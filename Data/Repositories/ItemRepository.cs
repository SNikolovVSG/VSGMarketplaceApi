using Dapper;
using Data.ViewModels;
using Data.Repositories.Interfaces;
using Data.Models;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Data.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly string connectionString;

        private readonly IImageRepository imageRepository;

        public ItemRepository(IConfiguration configuration, IImageRepository imageRepository)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");

            this.imageRepository = imageRepository;
        }

        public async Task<string> AddAsync(Item item)
        {
            using var connection = new SqlConnection(connectionString);
            string addItemSQL = "INSERT INTO Items (code, name, price, category, quantity, quantityForSale, description, imageURL, imagePublicId) VALUES (@Code, @Name, @Price, @Category, @Quantity, @QuantityForSale, @Description, @ImageURL, @ImagePublicId);";

            int changesByAddingItem = await connection.ExecuteAsync(addItemSQL, item);

            return changesByAddingItem > 0 ? Constants.Ok : Constants.DatabaseError;
        }

        public async Task<string> DeleteAsync(int code)
        {
            using var connection = new SqlConnection(connectionString);
            int changesByDeletingItem = await connection.ExecuteAsync("DELETE FROM Items WHERE code = @Code", new { Code = code });

            return changesByDeletingItem > 0 ? Constants.Ok : Constants.DatabaseError;
        }

        public async Task<Item> GetByCode(int code)
        {
            using var connection = new SqlConnection(connectionString);
            Item item = await connection.QueryFirstOrDefaultAsync<Item>("SELECT * FROM Items WHERE code = @Code", new { Code = code });

            return item;
        }

        public async Task<IEnumerable<InventoryItemViewModel>> GetInventoryItemsAsync()
        {
            using var connection = new SqlConnection(connectionString);

            var selectAllItemsSQL = "SELECT * FROM Items";
            var items = await connection.QueryAsync<InventoryItemViewModel>(selectAllItemsSQL);

            return items;
        }

        public async Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int code)
        {
            using var connection = new SqlConnection(connectionString);

            var selectItemByCodeSQL = "SELECT * FROM Items WHERE code = @Code";
            var item = await connection.QueryFirstAsync<MarketplaceByIdItemViewModel>(selectItemByCodeSQL, new { Code = code });

            if (item == null || item.QuantityForSale <= 0) { return null; }

            return item;
        }

        public async Task<IEnumerable<MarketplaceItemViewModel>> GetMarketplaceItemsAsync()
        {
            using var connection = new SqlConnection(connectionString);

            var selectAllMarketplaceItemsSQL = "SELECT * FROM Items WHERE quantityForSale > 0";
            var items = await connection.QueryAsync<MarketplaceItemViewModel>(selectAllMarketplaceItemsSQL);

            return items;
        }

        public async Task<string> UpdateAsync(Item item)
        {
            using var connection = new SqlConnection(connectionString);

            string updateItemSQL = "UPDATE Items SET name = @Name, price = @Price, category = @Category, quantity = @Quantity, quantityForSale = @QuantityForSale, description = @Description, imageURL = @ImageURl, imagePublicId = @ImagePublicId WHERE code = @code";
            int result = await connection.ExecuteAsync(updateItemSQL, item);

            return result > 0 ? Constants.Ok : Constants.DatabaseError;
        }

        public async Task<string[]> UpdateImage(ItemAddModelWithFormFile inputItem, int code)
        {
            using var connection = new SqlConnection(connectionString);

            string imagePublicIdSQL = "SELECT imagePublicId FROM Items WHERE code = @Code";

            string publicId = await connection.QueryFirstOrDefaultAsync<string>(imagePublicIdSQL, new { Code = code });

            string[] imageData = await imageRepository.UpdateImageAsync(inputItem.Image, publicId);

            return imageData;
        }
    }
}

