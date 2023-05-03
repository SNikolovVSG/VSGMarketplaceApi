namespace VSGMarketplaceApi.Data.Models
{
    public class Constants
    {
        public static string Pending = "Pending";
        public static string Finished = "Finished";
        public static string Ok = "OK";
        public static string DatabaseError = "Database Error";
    }

    public enum ItemCategory
    {
        Laptops,
        Computers,
        Desktops,
        Keyboards,
        Mouses,
        Desks,
        Headsets
    }
}
