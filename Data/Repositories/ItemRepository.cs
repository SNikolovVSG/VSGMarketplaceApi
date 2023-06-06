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
            string addItemSQL = "INSERT INTO Items (code, name, price, category, quantity, quantityForSale, availableQuantity, description, location, imageURL, imagePublicId) VALUES (@Code, @Name, @Price, @Category, @Quantity, @QuantityForSale, @AvailableQuantity, @Description, @Location, @ImageURL, @ImagePublicId);";

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

        public async Task<IEnumerable<Item>> GetInventoryItemsAsync()
        {
            using var connection = new SqlConnection(connectionString);

            string selectAllItemsSQL = "SELECT * FROM Items";
            var items = await connection.QueryAsync<Item>(selectAllItemsSQL);

            return items;
        }

        public async Task<Item> GetMarketplaceItemAsync(int id)
        {
            using var connection = new SqlConnection(connectionString);

            string selectItemByCodeSQL = "SELECT * FROM Items WHERE id = @Id";
            Item item = await connection.QueryFirstOrDefaultAsync<Item>(selectItemByCodeSQL, new { Id = id });

            if (item == null || item.QuantityForSale <= 0) { return null; }

            return item;
        }

        public async Task<IEnumerable<Item>> GetMarketplaceItemsAsync()
        {
            using var connection = new SqlConnection(connectionString);

            string selectAllMarketplaceItemsSQL = "SELECT * FROM Items WHERE quantityForSale > 0";
            IEnumerable<Item> items = await connection.QueryAsync<Item>(selectAllMarketplaceItemsSQL);

            return items;
        }

        public async Task<string> UpdateAsync(Item item, int id)
        {
            using var connection = new SqlConnection(connectionString);

            string updateItemSQL = $"UPDATE Items SET code = @Code, name = @Name, price = @Price, category = @Category, quantity = @Quantity, quantityForSale = @QuantityForSale, availableQuantity = @AvailableQuantity, description = @Description, location = @Location, imageURL = @ImageURL, imagePublicId = @ImagePublicId WHERE Id = @Id";
            int result = await connection.ExecuteAsync
                (updateItemSQL, new { item.Code, item.Name, item.Price, item.Category, item.Quantity, item.QuantityForSale, item.Description, item.ImageURL, item.ImagePublicId, item.Location, Id = id });

            return result > 0 ? Constants.Ok : Constants.DatabaseError;
        }

        public async Task<string> UpdateAsyncWithoutImageChangesAsync(Item item, int id)
        {
            using var connection = new SqlConnection(connectionString);

            string updateItemSQL = $"UPDATE Items SET code = @Code, name = @Name, price = @Price, category = @Category, quantity = @Quantity, quantityForSale = @QuantityForSale, availableQuantity = @AvailableQuantity, description = @Description, location = @Location WHERE id = @Id";
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

        public async Task<int> GetQuantityFromOrdersAsync(int itemId)
        {
            using var connection = new SqlConnection(connectionString);

            string getQuantitySQL = "SELECT Quantity FROM Orders WHERE ItemId = @ItemId AND Status = @PendingStatus AND IsDeleted = 0";

            var quantities = await connection.QueryAsync<int>(getQuantitySQL, new { itemId, PendingStatus = Constants.Pending });
         
            int quantity = 0;

            foreach (var item in quantities) { quantity += item; }

            return quantity;
        }

        public async Task<int> ReduceAvailableQuantity(Loan loan)
        {
            using var connection = new SqlConnection(connectionString);
            string reduceAvailabeQuantitySQL = "UPDATE Items SET AvailableQuantity -= @Quantity, Quantity -= @Quantity WHERE Id = @ItemId";

            return await connection.ExecuteAsync(reduceAvailabeQuantitySQL, loan);
        }
    }
}

