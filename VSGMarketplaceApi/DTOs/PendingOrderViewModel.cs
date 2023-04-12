namespace VSGMarketplaceApi.DTOs
{
    public class PendingOrderViewModel
    {
        public int Code { get; set; }

        public int Quantity { get; set; }

        public double OrderPrice { get; set; }

        public string OrderedBy { get; set; }

        public DateTime OrderDate { get; set; }

        public string Status { get; set; }
    }
}
