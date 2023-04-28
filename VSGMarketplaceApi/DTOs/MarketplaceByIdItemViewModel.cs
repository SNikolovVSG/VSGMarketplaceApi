﻿namespace VSGMarketplaceApi.DTOs
{
    public class MarketplaceByIdItemViewModel
    {
        public int Code { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public double Price { get; set; }

        public int QuantityForSale { get; set; }

        public string Description { get; set; }

        public string ImageURL { get; set; }
    }
}
