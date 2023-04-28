using AutoMapper;
using Dapper;
using FluentValidation;
using System.Data.SqlClient;
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

        public async Task<int> AddAsync(ItemAddModelString inputItem)
        {
            var item = mapper.Map<ItemAddModel>(inputItem);

            if (item == null) { return 0; };

            var result = validator.Validate(mapper.Map<Item>(item));

            if (!result.IsValid) { return 0; }

            using var connection = new SqlConnection(connectionString);

            if (item.Image != null)
            {
                var imageData = await this.imageRepository.UploadImageAsync(item.Image);

                if (imageData.IsNullOrEmpty()) { return 0; }

                item.ImageURL = imageData[0];
                item.ImagePublicId = imageData[1];
            }

            string addItemSQL = "insert into items (name, price, category, quantity, quantityForSale, description, imageURL, imagePublicId) values (@Name, @Price, @Category, @Quantity, @QuantityForSale, @Description, @ImageURL, @ImagePublicId)";
            int changesByAddingItem = await connection.ExecuteAsync(addItemSQL, item);

            return changesByAddingItem;
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

            var selectAllItemsSQL = "select * from Items";
            var items = await connection.QueryAsync<InventoryItemViewModel>(selectAllItemsSQL);

            return items;
        }

        public async Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int code)
        {
            using var connection = new SqlConnection(connectionString);

            var selectItemByCodeSQL = "select * from Items where code = @Code";
            var item = await connection.QueryFirstAsync<MarketplaceByIdItemViewModel>(selectItemByCodeSQL, new { Code = code });

            if (item == null || item.QuantityForSale <= 0) { return null; }

            return item;
        }

        public async Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync()
        {
            using var connection = new SqlConnection(connectionString);

            var selectAllMarketplaceItemsSQL = "select * from Items where quantityForSale > 0";
            var items = await connection.QueryAsync<MarketplaceItemViewModel>(selectAllMarketplaceItemsSQL);

            return items;
        }

        public async Task<int> UpdateAsync(ItemAddModelString inputItem, int code)
        {
            var editItem = mapper.Map<Item>(inputItem);
            editItem.Code = code;

            var validationResult = validator.Validate(editItem);
            if (!validationResult.IsValid) { return 0; }

            using var connection = new SqlConnection(connectionString);
            if (inputItem.Image != null)
            {
                var imagePublicIdSQL = "select imagePublicId from items where code = @Code";
                var publicId = await connection.QueryFirstAsync<string>(imagePublicIdSQL, new { Code = code });

                var imageData = await this.imageRepository.UpdateImageAsync(inputItem.Image, publicId);

                editItem.ImageURL = imageData[0];
                editItem.ImagePublicId = imageData[1];
            }

            int result = 0;
            try
            {
                var updateItemSQL = "update items set name = @Name, price = @Price, category = @Category, quantity = @Quantity, quantityForSale = @QuantityForSale, description = @Description, imageURL = @ImageURl, imagePublicId = @ImagePublicId where code = @code";
                result = await connection.ExecuteAsync(updateItemSQL, editItem);
            }
            catch (Exception)
            {
                return 0;
            }

            return result;
        }
    }
}

