using Data.ViewModels;

namespace Services.Interfaces
{
    public interface IItemsService
    {
        public Task<IEnumerable<InventoryItemViewModel>> GetInventoryItemsAsync();

        public Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync();

        public Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int id);

        public Task<string> AddAsync(ItemAddModelWithFormFile inputItem);

        public Task<string> DeleteAsync(int id);

        public Task<string> UpdateAsync(ItemAddModelWithFormFile inputItem, int id);
    }
}
