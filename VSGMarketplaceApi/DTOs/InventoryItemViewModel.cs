namespace VSGMarketplaceApi.DTOs
{
    public class InventoryItemViewModel
    {
        public int Code { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public int QuantityForSale { get; set; }

        public int Quantity { get; set; }

        public string ImageURL { get; set; }
    }
}
