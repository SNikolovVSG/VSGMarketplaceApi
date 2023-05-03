using VSGMarketplaceApi.DTOs;

namespace VSGMarketplaceApi.Data.Repositories.Interfaces
{
    public interface IItemRepository
    {
        Task<int> AddAsync(ItemAddModelString item);
        Task<string> AddAsyncTest(ItemAddModelString inputItem);
        Task<int> UpdateAsync(ItemAddModelString item, int code);
        Task<IEnumerable<InventoryItemViewModel>> GetInventoryItemsAsync();
        Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync();
        Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int code);
        Task<int> DeleteAsync(int code);
    }
}
