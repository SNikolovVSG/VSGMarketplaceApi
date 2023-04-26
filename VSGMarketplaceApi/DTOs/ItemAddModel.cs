namespace VSGMarketplaceApi.DTOs
{
    public class ItemAddModel
    {
        public string Name { get; set; }

        public double Price { get; set; }

        public string Category { get; set; }

        public int Quantity { get; set; }

        public int QuantityForSale { get; set; }

        public string Description { get; set; }

        public IFormFile Image { get; set; }
    }
}
