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

        public const string PENDING_ORDERS_CACHE_KEY = "PendingOrders";
        public const string PENDING_ORDER_CACHE_KEY = "PendingOrder";
        public const string MY_ORDERS_CACHE_KEY = "MyOrders";

        public const string INVENTORY_ITEMS_CACHE_KEY = "InventoryItems";
        public const string MARKETPLACE_ITEMS_CACHE_KEY = "MarketplaceItems";
        public const string MARKETPLACE_ITEM_CACHE_KEY = "MarketplaceItem";

        public const string DatabaseName = "MarketPlaceSpartak";
        public static readonly string[] LocationsArray = new string[] { "Home", "Veliko Tarnovo", "Plovdiv" };
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
