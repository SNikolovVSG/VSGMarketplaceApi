namespace Data.Models
{
    public class Constants
    {
        public const string Pending = "Pending";
        public const string Finished = "Finished";

        public const string Ok = "OK";
        public const string DatabaseError = "Database Error";
        public const string ValidationError = "Validation Error";

        public const string AdminGroup = "f2123818-3d51-4fe4-990b-b072a80da143";
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
