namespace VSGMarketplaceApi.ViewModels
{
    public class ItemAddModel
    {
        public int Code { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }

        public string Category { get; set; }

        public int Quantity { get; set; }

        public int QuantityForSale { get; set; }

        public string Description { get; set; }

        public IFormFile Image { get; set; }

        public string? ImageURL { get; set; }

        public string ImagePublicId { get; set; }
    }

    public class ItemAddModelString
    {
        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? Price { get; set; }

        public string? Category { get; set; }

        public string? Quantity { get; set; }

        public string? QuantityForSale { get; set; }

        public string? Description { get; set; }

        public IFormFile? Image { get; set; }

        public string? ImageURL { get; set; }
    }
}
