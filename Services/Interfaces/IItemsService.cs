using Data.ViewModels;

namespace Services.Interfaces
{
    public interface IItemsService
    {
        public Task<IEnumerable<InventoryItemViewModel>> GetInventoryItemsAsync();

        public Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync();

        public Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int code);

        public Task<string> AddAsync(ItemAddModelString inputItem);

        public Task<string> DeleteAsync(int code);

        public Task<string> UpdateAsync(ItemAddModelString inputItem, int code);
    }
}
