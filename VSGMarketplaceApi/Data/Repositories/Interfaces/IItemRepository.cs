using VSGMarketplaceApi.ViewModels;

namespace VSGMarketplaceApi.Data.Repositories.Interfaces
{
    public interface IItemRepository
    {
        Task<string> AddAsync(ItemAddModelString inputItem);
        Task<string> UpdateAsync(ItemAddModelString item, int code);
        Task<IEnumerable<InventoryItemViewModel>> GetInventoryItemsAsync();
        Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync();
        Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int code);
        Task<string> DeleteAsync(int code);
    }
}
