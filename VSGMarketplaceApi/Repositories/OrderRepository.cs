using Dapper;
using FluentValidation;
using Microsoft.Data.SqlClient;
using VSGMarketplaceApi.DTOs;
using VSGMarketplaceApi.Models;

namespace VSGMarketplaceApi.Repositories.Interfaces
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IConfiguration configuration;
        private readonly IValidator<Order> validator;

        public OrderRepository(IConfiguration configuration, IValidator<Order> validator)
        {
            this.configuration = configuration;
            this.validator = validator;
        }

        public async Task<int> AddAsync(NewOrderAddModel input)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));

            Item item = new Item();

            try
            {
                item = await connection.QueryFirstAsync<Item>("select * from items where code = @Code", new { Code = input.ItemCode });
            }
            catch (Exception)
            {
                return 0;
            }

            bool checkForQuantity = item.QuantityForSale < input.Quantity;
            if (checkForQuantity)
            {
                return 0;
            }

            var orderPrice = item.Price * input.Quantity;

            string userEmail = "";
            try
            {
                userEmail = await connection.QueryFirstAsync<string>("select email from users where id = @Id", new { Id = input.UserId });
            }
            catch (Exception)
            {
                return 0;
            }

            var order = new Order
            {
                ItemCode = input.ItemCode,
                Name = item.Name,
                Quantity = input.Quantity,
                OrderPrice = orderPrice,
                OrderedBy = userEmail,
                OrderDate = DateTime.Now.Date,
                Status = Constants.Pending,
                UserId = input.UserId,
                IsDeleted = false,
            };

            var result = validator.Validate(order);

            if (!result.IsValid) { return 0; }

            var insertSQL =
                "insert into Orders (ItemCode, Name, Quantity, OrderPrice, OrderedBy, OrderDate, Status, UserId, IsDeleted) values (@ItemCode, @Name, @Quantity, @OrderPrice, @OrderedBy, @OrderDate, @Status, @UserId, @IsDeleted)";

            var updateSQL = "update items set quantityForSale = @Count where code = @ItemCode";

            var updatedCount = item.QuantityForSale - input.Quantity;

            var changesByAddingOrder = await connection.ExecuteAsync(insertSQL, order);

            var changesByUpdatingItemQuantity = await connection.ExecuteAsync(updateSQL, new { Count = updatedCount, ItemCode = input.ItemCode });

            return changesByAddingOrder + changesByUpdatingItemQuantity;
        }

        public async Task<int> CompleteAsync(int code)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));

            var completingSQL = "update Orders set status = @Status where code = @Code and IsDeleted = 0";

            var changesByCompleting = await connection.ExecuteAsync(completingSQL, new { Status = Constants.Finished, Code = code });
            return changesByCompleting;
        }

        public async Task<int> DeleteAsync(int code, int userId)
        {
            int changesByItemsQuantity = 0;
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            Order order = null;

            try
            {
                var selectOrderSQL = "Select * from orders where code = @Code and IsDeleted = 0";

                order = await connection.QueryFirstAsync<Order>(selectOrderSQL, new { Code = code });
            }
            catch (Exception)
            {
                return 0;
            }

            if (order == null || order.UserId != userId)
            {
                return 0;
            }

            if (order.Status == Constants.Pending)
            {
                var selectItemSQL = "Select * from items where code = @Code";
                var item = await connection.QueryFirstAsync<Item>(selectItemSQL, new { Code = order.ItemCode });

                if (item == null) { return 0; }

                item.QuantityForSale += order.Quantity;

                var updateItemQuantitySQL = "update items set QuantityForSale = @QuantityForSale where Code = @Code";
                changesByItemsQuantity = await connection.ExecuteAsync(updateItemQuantitySQL, item);
            }

            var deleteOrderSQL = "update orders set IsDeleted = 1 where code = @Code";
            int changesByOrderDelete = await connection.ExecuteAsync(deleteOrderSQL, new { Code = code });

            return changesByOrderDelete + changesByItemsQuantity;
        }

        public async Task<IEnumerable<PendingOrderViewModel>> GetAllPendingOrdersAsync()
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            var orders = await connection.QueryAsync<PendingOrderViewModel>("select * from orders where status = @Pending and IsDeleted = 0", new { Pending = Constants.Pending });
            return orders;
        }

        public async Task<Order> GetByCodeAsync(int code)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
            try
            {
                var order = await connection.QueryFirstAsync<Order>("select * from orders where code = @Code and IsDeleted = 0", new { Code = code });
                return order;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IEnumerable<MyOrdersViewModel>> GetByUserId(int userId)
        {
            using var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));

            try
            {
                var orders = await connection.QueryAsync<MyOrdersViewModel>
                    ("SELECT ItemCode, Name, Quantity, OrderPrice, OrderedBy, OrderDate, Status, UserId FROM [VSGMarketplace].[dbo].[Orders] AS orT  INNER JOIN Users as usS on usS.Email = orT.OrderedBy where usS.Id = @Id and IsDeleted = 0", new { Id = userId });
                return orders;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
