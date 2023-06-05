using Dapper;
using System.Data.SqlClient;
using Data.Models;
using Data.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Data.ViewModels;
using System.Transactions;

namespace Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string connectionString;

        public OrderRepository(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<string> BuyAsync(Order order, Item item)
        {
            string addOrderSQL =
                "INSERT INTO Orders (ItemId, ItemCode, Location, Name, Quantity, OrderPrice, OrderedBy, OrderDate, Status, IsDeleted) VALUES (@ItemId, @ItemCode, @Location, @Name, @Quantity, @OrderPrice, @OrderedBy, @OrderDate, @Status, @IsDeleted)";

            string updateItemQuantitySQL = "UPDATE Items SET quantityForSale = @Count WHERE Id = @ItemId";

            int updatedCount = item.QuantityForSale - order.Quantity;

            using var connection = new SqlConnection(connectionString);

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                int changes = await connection.ExecuteAsync(addOrderSQL, order);

                if (changes == 0)
                {
                    throw new Exception(Constants.DatabaseError + " on Order add");
                }

                changes = 0;
                changes += await connection.ExecuteAsync(updateItemQuantitySQL, new { Count = updatedCount, ItemId = order.ItemId });
                if (changes == 0)
                {
                    throw new Exception(Constants.DatabaseError + " on Item quantity update");
                }

                transactionScope.Complete();
                transactionScope.Dispose();
            }

            return Constants.Ok;
        }

        public async Task<string> CompleteAsync(int id)
        {
            Order order = await GetByIdAsync(id);
            string completeOrderSQL = "UPDATE Orders SET status = @Status where Id = @Id and IsDeleted = 0";

            string removeItemQuantitySQL = "UPDATE Items SET Quantity -= @Quantity where Id = @ItemId";

            using var connection = new SqlConnection(connectionString);
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                int changesByRemoveItemQuantity = await connection.ExecuteAsync(removeItemQuantitySQL, new { order.Quantity, order.ItemId });

                if (changesByRemoveItemQuantity == 0)
                {
                    throw new Exception("Error when removing item quantity!");
                }

                int changesByCompleting = await connection.ExecuteAsync(completeOrderSQL, new { Status = Constants.Finished, Id = id });

                if (changesByCompleting == 0)
                {
                    throw new Exception("Error when completing the order!");
                }

                transactionScope.Complete();
                transactionScope.Dispose();
            }

            return Constants.Ok;
        }

        public async Task<string> DeleteAsync(int id)
        {
            int changesByItemsQuantity = 0;
            using var connection = new SqlConnection(connectionString);

            string deleteOrderSQL = "UPDATE ORDERS SET IsDeleted = 1 WHERE Id = @Id";

            int changesByOrderDelete = await connection.ExecuteAsync(deleteOrderSQL, new { Id = id });

            return changesByOrderDelete + changesByItemsQuantity > 0 ? Constants.Ok : Constants.DatabaseError;
        }

        public async Task<IEnumerable<Order>> GetAllPendingOrdersAsync()
        {
            using var connection = new SqlConnection(connectionString);

            string allPendingOrdersSQL = "SELECT * FROM Orders WHERE status = @Pending AND IsDeleted = 0";
            var orders = await connection.QueryAsync<Order>(allPendingOrdersSQL, new { Constants.Pending });
            return orders;
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(connectionString);

            var getOrderById = "SELECT * FROM Orders WHERE Id = @Id AND IsDeleted = 0";
            var order = await connection.QueryFirstOrDefaultAsync<Order>(getOrderById, new { Id = id });

            return order;
        }

        public async Task<IEnumerable<Order>> GetByUserEmail(string userEmail)
        {
            using var connection = new SqlConnection(connectionString);

            try
            {
                var orders = await connection.QueryAsync<Order>
                    ("SELECT Id, ItemCode, Name, Quantity, OrderPrice, OrderedBy, OrderDate, Status FROM Orders WHERE OrderedBy = @Email AND IsDeleted = 0", new { Email = userEmail });
                return orders;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> RevertChangesFromPendingOrder(Order order)
        {
            using var connection = new SqlConnection(connectionString);

            string selectItemByCode = "SELECT * FROM Items WHERE Id = @Id";
            Item item = await connection.QueryFirstAsync<Item>(selectItemByCode, new { Id = order.ItemId });

            if (item == null) { throw new Exception("Invalid item Code"); }

            item.QuantityForSale += order.Quantity;

            string updateItemQuantitySQL = "UPDATE Items SET QuantityForSale = @QuantityForSale WHERE Id = @Id";
            int changesByItemsQuantity = await connection.ExecuteAsync(updateItemQuantitySQL, item);

            return Constants.Ok;
        }
    }
}
