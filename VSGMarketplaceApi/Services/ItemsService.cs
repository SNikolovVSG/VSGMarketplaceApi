using Microsoft.Extensions.Caching.Memory;
using VSGMarketplaceApi.Data.Models;
using VSGMarketplaceApi.Data.Repositories.Interfaces;
using VSGMarketplaceApi.Services.Interfaces;
using VSGMarketplaceApi.ViewModels;

namespace VSGMarketplaceApi.Services
{
    public class ItemsService : IItemsService
    {
        private readonly IItemRepository repository;
        private IMemoryCache memoryCache;

        private string INVENTORY_ITEMS_CACHE_KEY = "InventoryItems";
        private string MARKETPLACE_ITEMS_CACHE_KEY = "MarketplaceItems";
        private string MARKETPLACE_ITEM_CACHE_KEY = "MarketplaceItem";

        public ItemsService(IItemRepository repository, IMemoryCache memoryCache)
        {
            this.repository = repository;
            this.memoryCache = memoryCache;
        }

        public async Task<IEnumerable<InventoryItemViewModel>> GetInventoryItemsAsync()
        {
            if (memoryCache.TryGetValue(INVENTORY_ITEMS_CACHE_KEY, out IEnumerable<InventoryItemViewModel> items))
            {
                return items;
            }

            items = await repository.GetInventoryItemsAsync();
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(INVENTORY_ITEMS_CACHE_KEY, items, options);

            return items;
        }

        public async Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync()
        {
            if (memoryCache.TryGetValue(MARKETPLACE_ITEMS_CACHE_KEY, out IEnumerable<MarketplaceItemViewModel> items))
            {
                return items;
            }

            items = await repository.GetMarketplaceItemsAsync();
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(MARKETPLACE_ITEMS_CACHE_KEY, items, options);

            return items;
        }

        public async Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int code)
        {
            if (memoryCache.TryGetValue(MARKETPLACE_ITEM_CACHE_KEY + code.ToString(), out MarketplaceByIdItemViewModel item))
            {
                return item;
            }

            item = await repository.GetMarketplaceItemAsync(code);
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(MARKETPLACE_ITEM_CACHE_KEY + code, item, options);

            return item;
        }

        public async Task<string> AddAsync(ItemAddModelString inputItem)
        {
            string result = await this.repository.AddAsync(inputItem);
            
            if (result != Constants.Ok) 
            {
                return result;
            }

            memoryCache.Remove(INVENTORY_ITEMS_CACHE_KEY);
            return result;
        }

        public async Task<string> DeleteAsync(int code)
        {
            string result = await this.repository.DeleteAsync(code);
            
            if (result != Constants.Ok) 
            {
                return result;
            }

            memoryCache.Remove(INVENTORY_ITEMS_CACHE_KEY);
            return result;
        }

        public async Task<string> UpdateAsync(ItemAddModelString inputItem, int code)
        {
            string result = await this.repository.UpdateAsync(inputItem, code);

            if (result != Constants.Ok)
            {
                return result;
            }

            memoryCache.Remove(INVENTORY_ITEMS_CACHE_KEY);
            return result;
        }
    }
}
