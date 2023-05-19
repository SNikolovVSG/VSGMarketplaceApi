using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace Data
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public IDbConnection CreateConnection()
            => new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        public IDbConnection CreateMasterConnection()
            => new SqlConnection(_configuration.GetConnectionString("MasterConnection"));
    }
}
