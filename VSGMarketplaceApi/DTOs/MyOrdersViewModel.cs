namespace VSGMarketplaceApi.DTOs
{
    public class MyOrdersViewModel
    {
        public string Name { get; set; }

        public int Quantity { get; set; }

        public double OrderPrice { get; set; }

        public DateTime OrderDate { get; set; }

        public string Status { get; set; }
    }
}
