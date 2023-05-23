using Microsoft.Extensions.Caching.Memory;
using Data.Models;
using Data.Repositories.Interfaces;
using Services.Interfaces;
using Data.ViewModels;

namespace Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IOrderRepository repository;
        private IMemoryCache memoryCache;

        private string PENDING_ORDERS_CACHE_KEY = "PendingOrders";
        private string PENDING_ORDER_CACHE_KEY = "PendingOrder";
        private string MY_ORDERS_CACHE_KEY = "MyOrders";

        public OrdersService(IOrderRepository repository, IMemoryCache memoryCache)
        {
            this.repository = repository;
            this.memoryCache = memoryCache;
        }

        public async Task<string> BuyAsync(NewOrderAddModel input)
        {
            string result = await repository.BuyAsync(input);

            if (result != Constants.Ok)
            {
                return result;
            }

            memoryCache.Remove(PENDING_ORDERS_CACHE_KEY);
            memoryCache.Remove(MY_ORDERS_CACHE_KEY);

            return result;
        }

        public async Task<string> CompleteAsync(int code)
        {
            string result = await repository.CompleteAsync(code);

            if (result != Constants.Ok)
            {
                return result;
            }

            memoryCache.Remove(PENDING_ORDERS_CACHE_KEY);
            memoryCache.Remove(MY_ORDERS_CACHE_KEY);

            return result;
        }

        public async Task<string> DeleteAsync(int code, string userEmail)
        {
            string result = await repository.DeleteAsync(code, userEmail);

            if (result != Constants.Ok)
            {
                return result;
            }

            memoryCache.Remove(PENDING_ORDERS_CACHE_KEY);
            memoryCache.Remove(MY_ORDERS_CACHE_KEY);

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

        public async Task<IEnumerable<MyOrdersViewModel>> GetByUserEmail(string userEmail)
        {
            if (memoryCache.TryGetValue(MY_ORDERS_CACHE_KEY, out IEnumerable<MyOrdersViewModel> orders))
            {
                return orders;
            }

            orders = await repository.GetByUserEmail(userEmail);
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(MY_ORDERS_CACHE_KEY, orders, options);

            return orders;
        }
    }
}
