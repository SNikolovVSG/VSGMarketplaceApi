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
using System.Transactions;

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

        public ItemsService(IItemRepository repository, IMemoryCache memoryCache, IMapper mapper, IValidator<Item> validator, IImageRepository imageRepository, IConfiguration configuration, IOrderRepository orderRepository)
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

            IEnumerable<Item> repositoryItems = await repository.GetInventoryItemsAsync();
            List<InventoryItemViewModel> outputItems = mapper.Map(repositoryItems, new List<InventoryItemViewModel>());

            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(Constants.INVENTORY_ITEMS_CACHE_KEY, outputItems, options);

            return outputItems;
        }

        public async Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync()
        {
            if (memoryCache.TryGetValue(Constants.MARKETPLACE_ITEMS_CACHE_KEY, out IEnumerable<MarketplaceItemViewModel> items))
            {
                return items;
            }

            IEnumerable<Item> repositoryItems = await repository.GetMarketplaceItemsAsync();
            List<MarketplaceItemViewModel> outputItems = mapper.Map(repositoryItems, new List<MarketplaceItemViewModel>());

            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(Constants.MARKETPLACE_ITEMS_CACHE_KEY, outputItems, options);

            return outputItems;
        }

        public async Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int id)
        {
            if (memoryCache.TryGetValue(Constants.MARKETPLACE_ITEM_CACHE_KEY + id.ToString(), out MarketplaceByIdItemViewModel item))
            {
                return item;
            }

            Item repositoryItem = await repository.GetMarketplaceItemAsync(id);
            MarketplaceByIdItemViewModel ouputItem = mapper.Map<MarketplaceByIdItemViewModel>(repositoryItem);

            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(Constants.MARKETPLACE_ITEM_CACHE_KEY + id, ouputItem, options);

            return ouputItem;
        }

        public async Task<string> AddAsync(ItemAddModelWithFormFile inputItem)
        {
            ItemAddModel item = mapper.Map<ItemAddModel>(inputItem);

            if (item == null) { throw new Exception("Invalid item"); }
            else if (item.Image != null && !item.Image.ContentType.Contains("image")) { throw new Exception("Invalid image!"); }

            var validationResult = validator.Validate(mapper.Map<Item>(item));
            if (!validationResult.IsValid) { throw new Exception("Validation error"); }

            var maxAvailableQuantity = int.Parse(inputItem.Quantity) - int.Parse(inputItem.QuantityForSale);
            if (int.Parse(inputItem.AvailableQuantity) > maxAvailableQuantity)
            {
                throw new Exception("Available quantity must be less than quantity minus quantity for sale!");
            }

            bool checkIfExistsItemWithSameCode = await CheckIfItemWithSameCodeAndLocationExistsAsync(inputItem.Code, inputItem.Location);
            if (checkIfExistsItemWithSameCode) { throw new Exception("Item with same code and location exists!"); }

            if (!Constants.ItemCategories.Contains(item.Category))
            {
                throw new Exception("Invalid category!");
            }

            if (!Constants.ItemLocations.Contains(item.Location))
            {
                throw new Exception("Invalid location!");
            }

            string result = "";
            using var connection = new SqlConnection(connectionString);
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (item.Image != null)
                {
                    var imageData = await imageRepository.UploadImageAsync(item.Image);

                    if (string.IsNullOrEmpty(imageData[0]) || string.IsNullOrEmpty(imageData[1]))
                    { throw new Exception("Image error"); }

                    item.ImageURL = imageData[0];
                    item.ImagePublicId = imageData[1];
                }

                Item itemToRepository = mapper.Map<Item>(item);

                result = await this.repository.AddAsync(itemToRepository);

                if (result != Constants.Ok)
                {
                    throw new Exception(result);
                }

                transactionScope.Complete();
                transactionScope.Dispose();
            }

            memoryCache.Remove(Constants.INVENTORY_ITEMS_CACHE_KEY);
            memoryCache.Remove(Constants.MARKETPLACE_ITEMS_CACHE_KEY);
            return result;
        }

        public async Task<string> DeleteAsync(int id)
        {
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                string publicId = await GetImagePublicIdAsync(id);

                string result = await this.repository.DeleteAsync(id);

                if (result != Constants.Ok)
                {
                    throw new Exception(result);
                }

                var imageDeletionResult = await this.imageRepository.DeleteImageAsync(publicId);

                if (imageDeletionResult.Result.ToLower() != Constants.Ok.ToLower())
                {
                    throw new Exception(imageDeletionResult.Error.ToString());
                }

                transactionScope.Complete();
                transactionScope.Dispose();
            }

            memoryCache.Remove(Constants.INVENTORY_ITEMS_CACHE_KEY);
            memoryCache.Remove(Constants.MARKETPLACE_ITEMS_CACHE_KEY);
            return Constants.Ok;
        }

        private async Task<string> GetImagePublicIdAsync(int id)
        {
            var item = await this.repository.GetByIdAsync(id);

            return item.ImagePublicId;
        }

        public async Task<string> UpdateAsync(ItemAddModelWithFormFile inputItem, int id)
        {
            string result = "";

            if (int.Parse(inputItem.Quantity) < int.Parse(inputItem.QuantityForSale))
            {
                throw new Exception("Quantity for sale must be less than Quantity!");
            }

            var quantityOfOrdersWithThisItem = await this.repository.GetQuantityFromOrdersAsync(id);
            if (quantityOfOrdersWithThisItem > int.Parse(inputItem.Quantity))
            {
                throw new Exception("There are pending orders for this item and the quantity of them is bigger than the input quantity!");
            }

            switch (inputItem.ImageChanges)
            {
                case false:
                    result = await UpdateAsyncWithoutImageChanges(inputItem, id);
                    break;
                case true:
                    result = await UpdateAsyncWithImageChanges(inputItem, id);
                    break;
            }

            if (result != Constants.Ok)
            {
                throw new Exception(result);
            }

            memoryCache.Remove(Constants.INVENTORY_ITEMS_CACHE_KEY);
            memoryCache.Remove(Constants.MARKETPLACE_ITEMS_CACHE_KEY);
            return result;
        }

        public async Task<string> UpdateAsyncWithImageChanges(ItemAddModelWithFormFile inputItem, int id)
        {
            Item item = mapper.Map<Item>(inputItem);

            var validationResult = validator.Validate(mapper.Map<Item>(item));
            if (!validationResult.IsValid) { throw new Exception("Validation error"); }

            if (inputItem.Image != null)
            {
                string[] imageData = await this.repository.UpdateImageAsync(inputItem, id);

                item.ImageURL = imageData[0];
                item.ImagePublicId = imageData[1];
            }
            else
            {
                await this.repository.DeleteImageAsync(id);
                item.ImageURL = null;
                item.ImagePublicId = null;
            }

            string result = await this.repository.UpdateAsync(item, id);

            if (result != Constants.Ok)
            {
                throw new Exception(result);
            }

            memoryCache.Remove(Constants.INVENTORY_ITEMS_CACHE_KEY);
            return result;
        }

        public async Task<string> UpdateAsyncWithoutImageChanges(ItemAddModelWithFormFile inputItem, int id)
        {
            Item item = mapper.Map<Item>(inputItem);

            var validationResult = validator.Validate(mapper.Map<Item>(item));
            if (!validationResult.IsValid) { throw new Exception("Validation error"); }

            string result = await this.repository.UpdateAsyncWithoutImageChangesAsync(item, id);

            if (result != Constants.Ok)
            {
                throw new Exception(result);
            }

            memoryCache.Remove(Constants.INVENTORY_ITEMS_CACHE_KEY);
            return result;
        }

        private async Task<bool> CheckIfItemWithSameCodeAndLocationExistsAsync(string? code, string location)
        {
            using var connection = new SqlConnection(connectionString);

            var item = await connection.QueryFirstOrDefaultAsync
                ($"SELECT * FROM Items WHERE Code = @Code and Location = @Location", new { Code = code, Location = location });

            return item != null;
        }
    }
}
