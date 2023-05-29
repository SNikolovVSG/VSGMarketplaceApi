﻿using Data.Models;
using Data.ViewModels;

namespace Data.Repositories.Interfaces
{
    public interface IItemRepository
    {
        Task<string> AddAsync(Item item);
        Task<string> UpdateAsync(Item item);
        Task<IEnumerable<InventoryItemViewModel>> GetInventoryItemsAsync();
        Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync();
        Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int code);
        Task<string> DeleteAsync(int code);
        Task<string[]> UpdateImageAsync(ItemAddModelWithFormFile inputItem, int code);
        Task<Item> GetByCodeAsync(int code);
        Task<string> UpdateAsyncWithoutImageChangesAsync(Item item);
        Task DeleteImageAsync(int code);

    }
}
