using System.ComponentModel.DataAnnotations.Schema;

namespace VSGMarketplaceApi.Data.Models
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //public string FirstName { get; set; }

        //public string LastName { get; set; }

        //public int PhoneNumber { get; set; }

        public string Email { get; set; }

        public string Role { get; set; }

        public string Password { get; set; }
    }
}
