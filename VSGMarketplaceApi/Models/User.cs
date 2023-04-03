namespace VSGMarketplaceApi.Models
{
    public class User
    {
        public User()
        {
            this.Orders = new List<Order>();
        }

        public int Id { get; set; }

        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public string Email { get; set; }

        public int PhoneNumber { get; set; }

        public string Role { get; set; }

        public string Password { get; set; }

        public List<Order> Orders { get; set; }
    }
}
