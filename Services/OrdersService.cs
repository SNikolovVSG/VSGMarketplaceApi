using Microsoft.Extensions.Caching.Memory;
using Data.Models;
using Data.Repositories.Interfaces;
using Services.Interfaces;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using FluentValidation;

namespace Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IOrderRepository repository;
        private IMemoryCache memoryCache;
        private readonly IItemRepository itemRepository;
        private readonly IValidator<Order> validator;

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
                item = await itemRepository.GetByIdAsync(input.ItemId);
            }
            catch (Exception)
            {
                throw new Exception("Wrong item Id");
            }

            if (item == null)
            {
                throw new Exception("Wrong item Id");
            }

            bool checkForQuantity = item.QuantityForSale < input.Quantity;
            if (checkForQuantity)
            {
                throw new Exception("Not enough quantity");
            }

            var orderPrice = item.Price * input.Quantity;

            var order = new Order
            {
                ItemId = input.ItemId,
                ItemCode = item.Code,
                Name = item.Name,
                Quantity = input.Quantity,
                OrderPrice = orderPrice,
                OrderedBy = userEmail,
                OrderDate = DateTime.Now,
                Status = Constants.Pending,
                Location = item.Location,
                IsDeleted = false,
            };

            var validationResult = validator.Validate(order);

            if (!validationResult.IsValid) { throw new Exception(Constants.ValidationError); }

            string result = await repository.BuyAsync(order, item);

            if (result != Constants.Ok)
            {
                throw new Exception(result);
            }

            memoryCache.Remove(Constants.PENDING_ORDERS_CACHE_KEY);
            memoryCache.Remove(Constants.MY_ORDERS_CACHE_KEY + userEmail);
            memoryCache.Remove(Constants.INVENTORY_ITEMS_CACHE_KEY);
            memoryCache.Remove(Constants.MARKETPLACE_ITEMS_CACHE_KEY);

            return result;
        }

        public async Task<string> CompleteAsync(int id)
        {
            var userEmail = httpContextAccessor.HttpContext.User.Claims.First(x => x.Value.Contains("vsgbg.com")).Value;
            string result = await repository.CompleteAsync(id);

            if (result != Constants.Ok)
            {
                throw new Exception(result);
            }

            memoryCache.Remove(Constants.PENDING_ORDERS_CACHE_KEY);
            memoryCache.Remove(Constants.MY_ORDERS_CACHE_KEY + userEmail);

            return result;
        }

        public async Task<string> DeleteAsync(int id)
        {
            var userEmail = httpContextAccessor.HttpContext.User.Claims.First(x => x.Value.Contains("vsgbg.com")).Value;

            Order order;
            try
            {
                order = await GetByIdAsync(id);
            }
            catch (Exception)
            {
                throw new Exception("Wrong order");
            }

            if (order == null || order.OrderedBy != userEmail)
            {
                throw new Exception("Not your order!");
            }

            if (order.Status == Constants.Pending)
            {
                string revertResult = await this.repository.RevertChangesFromPendingOrder(order);
            }
            string result = await repository.DeleteAsync(order.Id);

            if (result != Constants.Ok)
            {
                throw new Exception(result);
            }

            memoryCache.Remove(Constants.PENDING_ORDERS_CACHE_KEY);
            memoryCache.Remove(Constants.MY_ORDERS_CACHE_KEY + userEmail);
            memoryCache.Remove(Constants.INVENTORY_ITEMS_CACHE_KEY);
            memoryCache.Remove(Constants.MARKETPLACE_ITEMS_CACHE_KEY);

            return result;
        }

        public async Task<IEnumerable<PendingOrderViewModel>> GetAllPendingOrdersAsync()
        {
            if (memoryCache.TryGetValue(Constants.PENDING_ORDERS_CACHE_KEY, out IEnumerable<PendingOrderViewModel> orders))
            {
                return orders;
            }

            orders = await repository.GetAllPendingOrdersAsync();
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(Constants.PENDING_ORDERS_CACHE_KEY, orders, options);

            return orders;
        }

        public async Task<Order> GetByIdAsync(int code)
        {
            if (memoryCache.TryGetValue(Constants.PENDING_ORDER_CACHE_KEY + code.ToString(), out Order order))
            {
                return order;
            }

            order = await repository.GetByIdAsync(code);
            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(Constants.PENDING_ORDER_CACHE_KEY + code, order, options);

            return order;
        }

        public async Task<IEnumerable<MyOrdersViewModel>> GetByUserEmail()
        {
            var userEmail = httpContextAccessor.HttpContext.User.Claims.First(x => x.Value.Contains("vsgbg.com")).Value;
            if (memoryCache.TryGetValue(Constants.MY_ORDERS_CACHE_KEY + userEmail, out IEnumerable<MyOrdersViewModel> orders))
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

                memoryCache.Set(Constants.MY_ORDERS_CACHE_KEY + userEmail, orders, options);
            }

            return orders;
        }
    }
}
