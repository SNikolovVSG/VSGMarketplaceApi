using VSGMarketplaceApi.DTOs;
using VSGMarketplaceApi.Models;

namespace VSGMarketplaceApi.Repositories.Interfaces
{
    public interface IItemRepository
    {
        Task<int> AddAsync(ItemAddModelString item);
        Task<int> UpdateAsync(ItemAddModelString item, int code);
        Task<IEnumerable<InventoryItemViewModel>> GetInventoryItemsAsync();
        Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync();
        Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int code);
        Task<int> DeleteAsync(int code);
    }
}
