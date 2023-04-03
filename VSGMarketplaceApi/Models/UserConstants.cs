namespace VSGMarketplaceApi.Models
{
    public class UserConstants
    {
        public static List<User> Users = new List<User>()
        {
            new User() {FirstName = "Jason", LastName ="Admmin",PhoneNumber = 0882657782, Email = "jason.admin@email.com", Password = "MyPass_w0rd", Role = "Administrator"},
             new User() {FirstName = "Elyse", LastName = "Lambert", PhoneNumber = 0876258441, Email = "elyse.seller@email.com", Password = "MyPass_w0rd", Role = "Seller"},
             new User() {FirstName = "Spartak", LastName = "Nikolov", PhoneNumber = 0886946040, Email = "spartak.36seller@email.com", Password = "123456", Role = "Seller"}
        };
    }
}
