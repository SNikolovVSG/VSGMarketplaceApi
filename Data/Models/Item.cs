﻿namespace Data.Models
{
    public class Item
    {
        public int Id { get; set; }

        public int Code { get; set; }

        public string Name { get; set; }

        public double Price { get; set; }

        public string Category { get; set; }

        public int Quantity { get; set; }

        public int QuantityForSale { get; set; }

        public string? Description { get; set; }
     
        public string Location { get; set; }

        public string? ImageURL { get; set; }

        public string? ImagePublicId { get; set; }
    }
}
