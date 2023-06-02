using Data.Models;
using Data.ViewModels;

namespace Data.Repositories.Interfaces
{
    public interface IItemRepository
    {
        Task<string> AddAsync(Item item);
        Task<string> UpdateAsync(Item item, int id);
        Task<string> UpdateAsyncWithoutImageChangesAsync(Item item, int id);
        Task<IEnumerable<Item>> GetInventoryItemsAsync();
        Task<IEnumerable<Item>> GetMarketplaceItemsAsync();
        Task<Item> GetMarketplaceItemAsync(int id);
        Task<string> DeleteAsync(int id);
        Task<string[]> UpdateImageAsync(ItemAddModelWithFormFile inputItem, int id);
        Task<Item> GetByIdAsync(int id);
        Task DeleteImageAsync(int id);
        Task<int> GetQuantityFromOrdersAsync(int itemId);
        Task<int> ReduceAvailableQuantity(Loan loan);
    }
}
