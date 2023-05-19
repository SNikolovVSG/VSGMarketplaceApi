using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Data.Models;
using Data.ViewModels;

namespace VSGMarketplaceApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration config;

        public LoginController(IConfiguration configuration)
        {
            this.config = configuration;
        }


        [AllowAnonymous]
        [HttpPost("~/Login")]
        public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
        {
            var user = await Authenticate(userLogin);

            if (user != null)
            {
                var token = Generate(user);

                string[] args = { token, user.Email };

                RedirectToAction("~/Marketplace");

                return Ok(args);
            }

            return NotFound("User not found");
        }


        [Authorize]
        [HttpPost("~/Logout")]
        public ActionResult Logout()
        {
            Response.Headers.Remove("Authorization");
            return Ok();
        }

        private string Generate(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(config["Jwt:Issuer"],
                config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<User> Authenticate(UserLogin userLogin)
        {
            using var connection = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            var currentUser = await connection.QueryFirstAsync<User>("select * from users where @Email = email and @Password = password", userLogin);

            if (currentUser != null) { return currentUser; }

            return null;
        }
    }
}
