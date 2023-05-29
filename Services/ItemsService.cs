using Microsoft.Extensions.Caching.Memory;
using Data.Models;
using Data.Repositories.Interfaces;
using Services.Interfaces;
using Data.ViewModels;
using AutoMapper;
using FluentValidation;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;

namespace Services
{
    public class ItemsService : IItemsService
    {
        private readonly IItemRepository repository;
        private IMemoryCache memoryCache;
        private IMapper mapper;
        private readonly IValidator<Item> validator;
        private readonly string connectionString;
        private readonly IConfiguration configuration;

        private readonly IImageRepository imageRepository;

        public ItemsService(IItemRepository repository, IMemoryCache memoryCache, IMapper mapper, IValidator<Item> validator, IImageRepository imageRepository, IConfiguration configuration)
        {
            this.repository = repository;
            this.memoryCache = memoryCache;
            this.mapper = mapper;
            this.validator = validator;
            this.imageRepository = imageRepository;
            this.configuration = configuration;

            this.connectionString = this.configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<InventoryItemViewModel>> GetInventoryItemsAsync()
        {
            if (memoryCache.TryGetValue(Constants.INVENTORY_ITEMS_CACHE_KEY, out IEnumerable<InventoryItemViewModel> items))
            {
                return items;
            }

            items = await repository.GetInventoryItemsAsync();
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(Constants.INVENTORY_ITEMS_CACHE_KEY, items, options);

            return items;
        }

        public async Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync()
        {
            if (memoryCache.TryGetValue(Constants.MARKETPLACE_ITEMS_CACHE_KEY, out IEnumerable<MarketplaceItemViewModel> items))
            {
                return items;
            }

            items = await repository.GetMarketplaceItemsAsync();
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(Constants.MARKETPLACE_ITEMS_CACHE_KEY, items, options);

            return items;
        }

        public async Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int code)
        {
            if (memoryCache.TryGetValue(Constants.MARKETPLACE_ITEM_CACHE_KEY + code.ToString(), out MarketplaceByIdItemViewModel item))
            {
                return item;
            }

            item = await repository.GetMarketplaceItemAsync(code);
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(Constants.MARKETPLACE_ITEM_CACHE_KEY + code, item, options);

            return item;
        }

        public async Task<string> AddAsync(ItemAddModelWithFormFile inputItem)
        {
            ItemAddModel item = mapper.Map<ItemAddModel>(inputItem);

            if (item == null || !item.Image.ContentType.Contains("image")) { throw new Exception("Invalid item"); };

            var validationResult = validator.Validate(mapper.Map<Item>(item));

            if (!validationResult.IsValid) { throw new Exception("Validation error"); }

            bool checkIfExistsItemWithSameCode = await CheckIfExistsItemWithSameCodeAsync(inputItem.Code);
            if (checkIfExistsItemWithSameCode) { throw new Exception("Item with same code exists!"); }

            using var connection = new SqlConnection(connectionString);

            if (item.Image != null)
            {
                var imageData = await imageRepository.UploadImageAsync(item.Image);

                if (string.IsNullOrEmpty(imageData[0]) || string.IsNullOrEmpty(imageData[1]))
                { throw new Exception("Image error"); }

                item.ImageURL = imageData[0];
                item.ImagePublicId = imageData[1];
            }

            //bool categoryExist = Enum.IsDefined(typeof(ItemCategory), item.Category);
            //if (!categoryExist) { return "Category error"; }

            Item itemToRepository = mapper.Map<Item>(item);

            string result = await this.repository.AddAsync(itemToRepository);

            if (result != Constants.Ok)
            {
                return result;
            }

            memoryCache.Remove(Constants.INVENTORY_ITEMS_CACHE_KEY);
            return result;
        }

        public async Task<string> DeleteAsync(int code)
        {
            string result = await this.repository.DeleteAsync(code);

            if (result != Constants.Ok)
            {
                return result;
            }

            memoryCache.Remove(Constants.INVENTORY_ITEMS_CACHE_KEY);
            return result;
        }

        public async Task<string> UpdateAsync(ItemAddModelWithFormFile inputItem, int code)
        {
            string result = "";

            switch (inputItem.ImageChanges)
            {
                case "false":
                    result = await UpdateAsyncWithoutImageChanges(inputItem, code);
                    break;
                case "true":
                    result = await UpdateAsyncWithImageChanges(inputItem, code);
                    break;
            }

            if (result != Constants.Ok)
            {
                return result;
            }

            memoryCache.Remove(Constants.INVENTORY_ITEMS_CACHE_KEY);
            return result;
        }

        public async Task<string> UpdateAsyncWithImageChanges(ItemAddModelWithFormFile inputItem, int code)
        {
            Item item = mapper.Map<Item>(inputItem);

            var validationResult = validator.Validate(mapper.Map<Item>(item));
            if (!validationResult.IsValid) { throw new Exception("Validation error"); }

            if (inputItem.Image != null)
            {
                string[] imageData = await this.repository.UpdateImageAsync(inputItem, code);

                item.ImageURL = imageData[0];
                item.ImagePublicId = imageData[1];
            }
            else
            {
                await this.repository.DeleteImageAsync(code);
                item.ImageURL = null;
                item.ImagePublicId = null;
            }

            string result = await this.repository.UpdateAsync(item);

            if (result != Constants.Ok)
            {
                return result;
            }

            memoryCache.Remove(Constants.INVENTORY_ITEMS_CACHE_KEY);
            return result;
        }

        public async Task<string> UpdateAsyncWithoutImageChanges(ItemAddModelWithFormFile inputItem, int code)
        {
            Item item = mapper.Map<Item>(inputItem);

            var validationResult = validator.Validate(mapper.Map<Item>(item));
            if (!validationResult.IsValid) { throw new Exception("Validation error"); }

            string result = await this.repository.UpdateAsyncWithoutImageChangesAsync(item);

            if (result != Constants.Ok)
            {
                return result;
            }

            memoryCache.Remove(Constants.INVENTORY_ITEMS_CACHE_KEY);
            return result;
        }

        private async Task<bool> CheckIfExistsItemWithSameCodeAsync(string? code)
        {
            using var connection = new SqlConnection(connectionString);

            var item = await connection.QueryFirstOrDefaultAsync($"SELECT * FROM Items WHERE Code = {code}");

            return item != null;
        }
    }
}
