using Microsoft.Extensions.Caching.Memory;
using Data.Models;
using Data.Repositories.Interfaces;
using Services.Interfaces;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using FluentValidation;
using CloudinaryDotNet.Actions;

namespace Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IOrderRepository repository;
        private IMemoryCache memoryCache;
        private readonly IItemRepository itemRepository;
        private readonly IValidator<Order> validator;

        private string PENDING_ORDERS_CACHE_KEY = "PendingOrders";
        private string PENDING_ORDER_CACHE_KEY = "PendingOrder";
        private string MY_ORDERS_CACHE_KEY = "MyOrders";

        private HttpContextAccessor httpContextAccessor;

        public OrdersService(IOrderRepository repository, IMemoryCache memoryCache, IItemRepository itemRepository, IValidator<Order> validator)
        {
            this.repository = repository;
            this.memoryCache = memoryCache;

            this.httpContextAccessor = new HttpContextAccessor();
            this.itemRepository = itemRepository;
            this.validator = validator;
        }

        public async Task<string> BuyAsync(NewOrderAddModel input)
        {
            var userEmail = httpContextAccessor.HttpContext.User.Claims.First(x => x.Value.Contains("vsgbg.com")).Value;

            Item item;
            try
            {
                item = await itemRepository.GetByCode(input.ItemCode);
            }
            catch (Exception)
            {
                return "Wrong item Code";
            }

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

            var validationResult = validator.Validate(order);

            if (!validationResult.IsValid) { return Constants.ValidationError; }

            string result = await repository.BuyAsync(order, item);

            if (result != Constants.Ok)
            {
                return result;
            }

            memoryCache.Remove(PENDING_ORDERS_CACHE_KEY);
            memoryCache.Remove(MY_ORDERS_CACHE_KEY + userEmail);

            return result;
        }

        public async Task<string> CompleteAsync(int code)
        {
            var userEmail = httpContextAccessor.HttpContext.User.Claims.First(x => x.Value.Contains("vsgbg.com")).Value;
            string result = await repository.CompleteAsync(code);

            if (result != Constants.Ok)
            {
                return result;
            }

            memoryCache.Remove(PENDING_ORDERS_CACHE_KEY);
            memoryCache.Remove(MY_ORDERS_CACHE_KEY + userEmail);

            return result;
        }

        public async Task<string> DeleteAsync(int code)
        {
            var userEmail = httpContextAccessor.HttpContext.User.Claims.First(x => x.Value.Contains("vsgbg.com")).Value;

            Order order;
            try
            {
                order = await GetByCodeAsync(code);
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
                string revertResult = await this.repository.RevertChangesFromPendingOrder(order);
            }
            string result = await repository.DeleteAsync(order.Code);

            if (result != Constants.Ok)
            {
                return result;
            }

            memoryCache.Remove(PENDING_ORDERS_CACHE_KEY);
            memoryCache.Remove(MY_ORDERS_CACHE_KEY + userEmail);

            return result;
        }

        public async Task<IEnumerable<PendingOrderViewModel>> GetAllPendingOrdersAsync()
        {
            if (memoryCache.TryGetValue(PENDING_ORDERS_CACHE_KEY, out IEnumerable<PendingOrderViewModel> orders))
            {
                return orders;
            }

            orders = await repository.GetAllPendingOrdersAsync();
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(PENDING_ORDERS_CACHE_KEY, orders, options);

            return orders;
        }

        public async Task<Order> GetByCodeAsync(int code)
        {
            if (memoryCache.TryGetValue(PENDING_ORDER_CACHE_KEY + code.ToString(), out Order order))
            {
                return order;
            }

            order = await repository.GetByCodeAsync(code);
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(PENDING_ORDER_CACHE_KEY + code, order, options);

            return order;
        }

        public async Task<IEnumerable<MyOrdersViewModel>> GetByUserEmail()
        {
            var userEmail = httpContextAccessor.HttpContext.User.Claims.First(x => x.Value.Contains("vsgbg.com")).Value;
            if (memoryCache.TryGetValue(MY_ORDERS_CACHE_KEY + userEmail, out IEnumerable<MyOrdersViewModel> orders))
            {
                return orders;
            }

            orders = await repository.GetByUserEmail(userEmail);
            if (orders != null)
            {
                var options = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                };

                memoryCache.Set(MY_ORDERS_CACHE_KEY + userEmail, orders, options);
            }

            return orders;
        }
    }
}
