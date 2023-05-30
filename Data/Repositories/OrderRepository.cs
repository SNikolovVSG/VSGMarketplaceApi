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

        //working
        public async Task<string> BuyAsync(Order order, Item item)
        {
            var addOrderSQL =
                "INSERT INTO Orders (ItemCode, Name, Quantity, OrderPrice, OrderedBy, OrderDate, Status, IsDeleted) VALUES (@ItemCode, @Name, @Quantity, @OrderPrice, @OrderedBy, @OrderDate, @Status, @IsDeleted)";

            var updateItemQuantitySQL = "UPDATE Items SET quantityForSale = @Count WHERE code = 0";
            //var updateItemQuantitySQL = "UPDATE Items SET quantityForSale = @Count WHERE code = @ItemCode";

            var updatedCount = item.QuantityForSale - order.Quantity;

            using var connection = new SqlConnection(connectionString);
            
            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                int changes = await connection.ExecuteAsync(addOrderSQL, order);

                if (changes == 0)
                {
                    throw new Exception(Constants.DatabaseError + " on Order add");
                }

                changes = 0;
                changes += await connection.ExecuteAsync(updateItemQuantitySQL, new { Count = updatedCount, ItemCode = item.Code });
                if (changes == 0)
                {
                    throw new Exception(Constants.DatabaseError + " on Item quantity update");
                }

                transactionScope.Complete();
                transactionScope.Dispose();
            }

            return Constants.Ok;
        }

        public async Task<string> CompleteAsync(int code)
        {
            using var connection = new SqlConnection(connectionString);

            var completeOrderSQL = "UPDATE Orders SET status = @Status where code = @Code and IsDeleted = 0";
            var changesByCompleting = await connection.ExecuteAsync(completeOrderSQL, new { Status = Constants.Finished, Code = code });
            return changesByCompleting > 0 ? Constants.Ok : Constants.DatabaseError;
        }

        public async Task<string> DeleteAsync(int code)
        {
            int changesByItemsQuantity = 0;
            using var connection = new SqlConnection(connectionString);

            var deleteOrderSQL = "UPDATE ORDERS SET IsDeleted = 1 WHERE code = @Code";

            int changesByOrderDelete = await connection.ExecuteAsync(deleteOrderSQL, new { Code = code });

            return changesByOrderDelete + changesByItemsQuantity > 0 ? Constants.Ok : Constants.DatabaseError;
        }

        public async Task<IEnumerable<PendingOrderViewModel>> GetAllPendingOrdersAsync()
        {
            using var connection = new SqlConnection(connectionString);

            var allPendingOrdersSQL = "SELECT * FROM Orders WHERE status = @Pending AND IsDeleted = 0";
            var orders = await connection.QueryAsync<PendingOrderViewModel>(allPendingOrdersSQL, new { Constants.Pending });
            return orders;
        }

        public async Task<Order> GetByCodeAsync(int code)
        {
            using var connection = new SqlConnection(connectionString);
            try
            {
                var getOrderByCode = "SELECT * FROM Orders WHERE code = @Code AND IsDeleted = 0";
                var order = await connection.QueryFirstAsync<Order>(getOrderByCode, new { Code = code });
                return order;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<MyOrdersViewModel>> GetByUserEmail(string userEmail)
        {
            using var connection = new SqlConnection(connectionString);

            try
            {
                var orders = await connection.QueryAsync<MyOrdersViewModel>
                    ("SELECT Code, ItemCode, Name, Quantity, OrderPrice, OrderedBy, OrderDate, Status FROM Orders  WHERE OrderedBy = @Email AND IsDeleted = 0", new { Email = userEmail });
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

            var selectItemByCode = "SELECT * FROM Items WHERE code = @Code";
            var item = await connection.QueryFirstAsync<Item>(selectItemByCode, new { Code = order.ItemCode });

            if (item == null) { throw new Exception("Invalid item Code"); }

            item.QuantityForSale += order.Quantity;

            var updateItemQuantitySQL = "UPDATE Items SET QuantityForSale = @QuantityForSale WHERE Code = @Code";
            int changesByItemsQuantity = await connection.ExecuteAsync(updateItemQuantitySQL, item);

            return Constants.Ok;
        }
    }
}
