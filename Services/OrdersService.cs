using Microsoft.Extensions.Caching.Memory;
using Data.Models;
using Data.Repositories.Interfaces;
using Services.Interfaces;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;

namespace Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IOrderRepository repository;
        private IMemoryCache memoryCache;

        private string PENDING_ORDERS_CACHE_KEY = "PendingOrders";
        private string PENDING_ORDER_CACHE_KEY = "PendingOrder";
        private string MY_ORDERS_CACHE_KEY = "MyOrders";
        private HttpContextAccessor httpContextAccessor;

        public OrdersService(IOrderRepository repository, IMemoryCache memoryCache)
        {
            this.repository = repository;
            this.memoryCache = memoryCache;

            this.httpContextAccessor = new HttpContextAccessor();
        }

        public async Task<string> BuyAsync(NewOrderAddModel input)
        {
            var userEmail = httpContextAccessor.HttpContext.User.Claims.First(x => x.Value.Contains("vsgbg.com")).Value;
            string result = await repository.BuyAsync(input, userEmail);

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
            string result = await repository.DeleteAsync(code, userEmail);

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
