using VSGMarketplaceApi.DTOs;
using VSGMarketplaceApi.Models;

namespace VSGMarketplaceApi.Repositories.Interfaces
{
    public interface IItemRepository
    {
        Task<int> AddAsync(ItemAddModel item);
        Task<int> UpdateAsync(ItemAddModel item, int code);
        Task<IEnumerable<InventoryItemViewModel>> GetInventoryItemsAsync();
        Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync();
        Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int code);
    }
}
