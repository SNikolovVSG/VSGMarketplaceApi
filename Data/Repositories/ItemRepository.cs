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
            string addItemSQL = "INSERT INTO Items (code, name, price, category, quantity, quantityForSale, description, location, imageURL, imagePublicId) VALUES (@Code, @Name, @Price, @Category, @Quantity, @QuantityForSale, @Description, @Location, @ImageURL, @ImagePublicId);";

            int changesByAddingItem = await connection.ExecuteAsync(addItemSQL, item);

            return changesByAddingItem > 0 ? Constants.Ok : Constants.DatabaseError;
        }

        public async Task<string> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(connectionString);
            int changesByDeletingItem = await connection.ExecuteAsync("DELETE FROM Items WHERE Id = @Id", new { Id = id });

            return changesByDeletingItem > 0 ? Constants.Ok : Constants.DatabaseError;
        }

        public async Task<Item> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(connectionString);
            Item item = await connection.QueryFirstOrDefaultAsync<Item>("SELECT * FROM Items WHERE Id = @Id", new { Id = id });

            return item;
        }

        public async Task<IEnumerable<InventoryItemViewModel>> GetInventoryItemsAsync()
        {
            using var connection = new SqlConnection(connectionString);

            var selectAllItemsSQL = "SELECT * FROM Items";
            var items = await connection.QueryAsync<InventoryItemViewModel>(selectAllItemsSQL);

            return items;
        }

        public async Task<MarketplaceByIdItemViewModel> GetMarketplaceItemAsync(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var selectItemByCodeSQL = "SELECT * FROM Items WHERE id = @Id";
            var item = await connection.QueryFirstAsync<MarketplaceByIdItemViewModel>(selectItemByCodeSQL, new { Id = id });

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

        public async Task<string> UpdateAsync(Item item, int id)
        {
            using var connection = new SqlConnection(connectionString);

            string updateItemSQL = $"UPDATE Items SET code = @Code, name = @Name, price = @Price, category = @Category, quantity = @Quantity, quantityForSale = @QuantityForSale, description = @Description, location = @Location, imageURL = @ImageURL, imagePublicId = @ImagePublicId WHERE Id = @Id";
            int result = await connection.ExecuteAsync
                (updateItemSQL, new { item.Code, item.Name, item.Price, item.Category, item.Quantity, item.QuantityForSale, item.Description, item.ImageURL, item.ImagePublicId, item.Location, Id = id });

            return result > 0 ? Constants.Ok : Constants.DatabaseError;
        }

        public async Task<string> UpdateAsyncWithoutImageChangesAsync(Item item, int id)
        {
            using var connection = new SqlConnection(connectionString);

            string updateItemSQL = $"UPDATE Items SET code = @Code, name = @Name, price = @Price, category = @Category, quantity = @Quantity, quantityForSale = @QuantityForSale, description = @Description, location = @Location WHERE id = @Id";
            int result = await connection.ExecuteAsync
                (updateItemSQL, new { item.Code, item.Name, item.Price, item.Category, item.Quantity, item.QuantityForSale, item.Description, item.Location, Id = id });

            return result > 0 ? Constants.Ok : Constants.DatabaseError;
        }

        public async Task<string[]> UpdateImageAsync(ItemAddModelWithFormFile inputItem, int id)
        {
            using var connection = new SqlConnection(connectionString);

            string imagePublicIdSQL = "SELECT imagePublicId FROM Items WHERE Id = @Id";

            string publicId = await connection.QueryFirstOrDefaultAsync<string>(imagePublicIdSQL, new { Id = id });

            string[] imageData = await imageRepository.UpdateImageAsync(inputItem.Image, publicId);

            return imageData;
        }

        public async Task DeleteImageAsync(int id)
        {
            using var connection = new SqlConnection(connectionString);
            string imagePublicIdSQL = "SELECT imagePublicId FROM Items WHERE Id = @Id";
            string publicId = await connection.QueryFirstOrDefaultAsync<string>(imagePublicIdSQL, new { Id = id });

            await imageRepository.DeleteImageAsync(publicId);
        }
    }
}

