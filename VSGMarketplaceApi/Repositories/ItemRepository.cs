using AutoMapper;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Dapper;
using FluentValidation;
using System.Data.SqlClient;
using VSGMarketplaceApi.Controllers;
using VSGMarketplaceApi.DTOs;
using VSGMarketplaceApi.Models;
using VSGMarketplaceApi.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace VSGMarketplaceApi.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly IConfiguration configuration;
        private readonly IMapper mapper;
        private readonly IValidator<Item> validator;
        private readonly string connectionString;

        private readonly IImageRepository imageRepository;

        public ItemRepository(IConfiguration configuration, IMapper mapper, IValidator<Item> validator, IImageRepository imageRepository)
        {
            this.configuration = configuration;
            this.mapper = mapper;
            this.validator = validator;

            this.connectionString = this.configuration.GetConnectionString("DefaultConnection");

            this.imageRepository = imageRepository;
        }

        public async Task<int> AddAsync(ItemAddModel item)
        {
            if (item == null) { return 0; };

            var result = validator.Validate(mapper.Map<Item>(item));

            if (!result.IsValid) { return 0; }

            using var connection = new SqlConnection(connectionString);
            int changesByAddingItem = await connection.ExecuteAsync("insert into items (name, price, category, quantity, quantityForSale, description) values (@Name, @Price, @Category, @Quantity, @QuantityForSale, @Description)", item);

            var itemCode = connection.QueryFirst<int>("select code from items where name = @Name", new { Name = item.Name });

            var imageURL = this.imageRepository.UploadImage(item.Image, item.Name, itemCode);

            if (imageURL.IsNullOrEmpty()) { return 0; }

            int changesByAddingImageToTheItem = await connection.ExecuteAsync("update items set imageURL = @ImageURL where Code = @Code", new { ImageURL = imageURL, Code = itemCode });

            if (changesByAddingImageToTheItem == 0)
            {
                return 0;
            }

            return changesByAddingItem + changesByAddingImageToTheItem;
        }

        public async Task<int> DeleteAsync(int code)
        {
            using var connection = new SqlConnection(connectionString);
            var result = await connection.ExecuteAsync("delete from items where code = @Code", new { Code = code });

            return result;
        }

        public async Task<IEnumerable<InventoryItemViewModel>> GetInventoryItemsAsync()
        {
            using var connection = new SqlConnection(connectionString);
            var items = await connection.QueryAsync<InventoryItemViewModel>("select * from Items");

            return items;
        }

        public async Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int code)
        {
            using var connection = new SqlConnection(connectionString);
            var item = await connection.QueryFirstAsync<MarketplaceByIdItemViewModel>("select * from Items where code = @Code", new { Code = code });
            if (item == null || item.QuantityForSale <= 0)
            {
                return null;
            }
            return item;
        }

        public async Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync()
        {
            using var connection = new SqlConnection(connectionString);
            var items = await connection.QueryAsync<MarketplaceItemViewModel>("select * from Items where quantityForSale > 0");
            return items;
        }

        public async Task<int> UpdateAsync(ItemAddModel item, int code)
        {
            var editItem = mapper.Map<Item>(item);
            editItem.Code = code;

            var validationResult = validator.Validate(editItem);
            if (!validationResult.IsValid) { return 0; }

            using var connection = new SqlConnection(connectionString);
            int result = 0;
            try
            {
                result = await connection.ExecuteAsync
                    ("update items set " +
                    "name = @Name, " +
                    "price = @Price, " +
                    "category = @Category, " +
                    "quantity = @Quantity, " +
                    "quantityForSale = @QuantityForSale, " +
                    "description = @Description " +
                    "where code = @code", editItem);
            }
            catch (Exception)
            {
                return 0;
            }

            return result;
        }
    }
}

