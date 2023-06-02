namespace Data.ViewModels
{
    public class InventoryItemViewModel
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public int Price { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public int QuantityForSale { get; set; }

        public int Quantity { get; set; }
        
        public int AvailableQuantity { get; set; }

        public string ImageURL { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }
    }
}
