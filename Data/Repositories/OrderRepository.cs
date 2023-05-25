using Dapper;
using FluentValidation;
using System.Data.SqlClient;
using Data.Models;
using Data.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Data.ViewModels;

namespace Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IConfiguration configuration;
        private readonly IValidator<Order> validator;
        private readonly string connectionString;

        public OrderRepository(IConfiguration configuration, IValidator<Order> validator)
        {
            this.configuration = configuration;
            this.validator = validator;
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<string> BuyAsync(NewOrderAddModel input, string userEmail)
        {
            using var connection = new SqlConnection(connectionString);

            Item item;

            try
            {
                var selectItemByCode = "SELECT * FROM dbo.Items WHERE code = @Code";
                item = await connection.QueryFirstAsync<Item>(selectItemByCode, new { Code = input.ItemCode });
            }
            catch (Exception)
            {
                return "Wrong item Code";
            }

            if (item == null) { return "There isn't item with this code."; }

            bool checkForQuantity = item.QuantityForSale < input.Quantity;
            if (checkForQuantity)
            {
                return "Not enough quantity";
            }

            var orderPrice = item.Price * input.Quantity;

            var order = new Order
            {
                ItemCode = input.ItemCode,
                Name = item.Name,
                Quantity = input.Quantity,
                OrderPrice = orderPrice,
                OrderedBy = userEmail,
                OrderDate = DateTime.Now,
                Status = Constants.Pending,
                IsDeleted = false,
            };

            var result = validator.Validate(order);

            if (!result.IsValid) { return Constants.ValidationError; }

            var addOrderSQL =
                "INSERT INTO dbo.Orders (ItemCode, Name, Quantity, OrderPrice, OrderedBy, OrderDate, Status, IsDeleted) VALUES (@ItemCode, @Name, @Quantity, @OrderPrice, @OrderedBy, @OrderDate, @Status, @IsDeleted)";

            var updateItemQuantitySQL = "UPDATE dbo.Items SET quantityForSale = @Count WHERE code = @ItemCode";

            var updatedCount = item.QuantityForSale - input.Quantity;

            var changesByAddingOrder = await connection.ExecuteAsync(addOrderSQL, order);

            var changesByUpdatingItemQuantity = await connection.ExecuteAsync(updateItemQuantitySQL, new { Count = updatedCount, input.ItemCode });

            if (changesByAddingOrder + changesByUpdatingItemQuantity > 0)
            {
                return Constants.Ok;
            }
            return Constants.DatabaseError;
        }

        public async Task<string> CompleteAsync(int code)
        {
            using var connection = new SqlConnection(connectionString);

            var completeOrderSQL = "UPDATE dbo.Orders SET status = @Status where code = @Code and IsDeleted = 0";
            var changesByCompleting = await connection.ExecuteAsync(completeOrderSQL, new { Status = Constants.Finished, Code = code });
            return changesByCompleting > 0 ? Constants.Ok : Constants.DatabaseError;
        }

        public async Task<string> DeleteAsync(int code, string userEmail)
        {
            int changesByItemsQuantity = 0;
            using var connection = new SqlConnection(connectionString);
            Order order;

            try
            {
                var selectOrderSQL = "SELECT * FROM dbo.Orders WHERE code = @Code AND IsDeleted = 0";
                order = await connection.QueryFirstAsync<Order>(selectOrderSQL, new { Code = code });
            }
            catch (Exception)
            {
                return "Wrong order";
            }

            if (order == null || order.OrderedBy != userEmail)
            {
                return "Not your order!";
            }

            if (order.Status == Constants.Pending)
            {
                var selectItemByCode = "SELECT * FROM dbo.Items WHERE code = @Code";
                var item = await connection.QueryFirstAsync<Item>(selectItemByCode, new { Code = order.ItemCode });

                if (item == null) { return "Invalid item Code"; }

                item.QuantityForSale += order.Quantity;

                var updateItemQuantitySQL = "UPDATE dbo.Items SET QuantityForSale = @QuantityForSale WHERE Code = @Code";
                changesByItemsQuantity = await connection.ExecuteAsync(updateItemQuantitySQL, item);
            }

            var deleteOrderSQL = "UPDATE ORDERS SET IsDeleted = 1 WHERE code = @Code";

            //var HARDdeleteOrderSQL = "delete from orders where code = @Code";

            int changesByOrderDelete = await connection.ExecuteAsync(deleteOrderSQL, new { Code = code });

            return changesByOrderDelete + changesByItemsQuantity > 0 ? Constants.Ok : Constants.DatabaseError;
        }

        public async Task<IEnumerable<PendingOrderViewModel>> GetAllPendingOrdersAsync()
        {
            using var connection = new SqlConnection(connectionString);

            var allPendingOrdersSQL = "SELECT * FROM dbo.Orders WHERE status = @Pending AND IsDeleted = 0";
            var orders = await connection.QueryAsync<PendingOrderViewModel>(allPendingOrdersSQL, new { Constants.Pending });
            return orders;
        }

        public async Task<Order> GetByCodeAsync(int code)
        {
            using var connection = new SqlConnection(connectionString);
            try
            {
                var getOrderByCode = "SELECT * FROM dbo.Orders WHERE code = @Code AND IsDeleted = 0";
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
                    ("SELECT Code, ItemCode, Name, Quantity, OrderPrice, OrderedBy, OrderDate, Status FROM dbo.Orders  WHERE OrderedBy = @Email AND IsDeleted = 0", new { Email = userEmail });
                return orders;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
