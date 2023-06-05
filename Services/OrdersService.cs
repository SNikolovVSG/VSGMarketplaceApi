using Microsoft.Extensions.Caching.Memory;
using Data.Models;
using Data.Repositories.Interfaces;
using Services.Interfaces;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using AutoMapper;

namespace Services
{
    public class OrdersService : IOrdersService
    {
        private HttpContextAccessor httpContextAccessor;
        private readonly IItemRepository itemRepository;
        private readonly IOrderRepository repository;
        private readonly IValidator<Order> validator;
        private IMemoryCache memoryCache;
        private IMapper mapper;


        public OrdersService
            (IOrderRepository repository, IMemoryCache memoryCache, IItemRepository itemRepository, IValidator<Order> validator, IMapper mapper)
        {
            this.repository = repository;
            this.memoryCache = memoryCache;
            this.mapper = mapper;

            this.httpContextAccessor = new HttpContextAccessor();
            this.itemRepository = itemRepository;
            this.validator = validator;
        }

        public async Task BuyAsync(NewOrderAddModel input)
        {
            var userEmail = httpContextAccessor.HttpContext.User.Claims.First(x => x.Value.Contains("vsgbg.com")).Value;

            Item item = await itemRepository.GetByIdAsync(input.ItemId);

            if (item == null)
            {
                throw new Exception("Wrong item Id");
            }
            else if (item.QuantityForSale < input.Quantity)
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

            if (!validationResult.IsValid) { throw new Exception(Constants.ValidationError + " " + validationResult.ToString()); }

            string result = await repository.BuyAsync(order, item);

            if (result != Constants.Ok)
            {
                throw new Exception(result);
            }

            memoryCache.Remove(Constants.PENDING_ORDERS_CACHE_KEY);
            memoryCache.Remove(Constants.MY_ORDERS_CACHE_KEY + userEmail);
            memoryCache.Remove(Constants.INVENTORY_ITEMS_CACHE_KEY);
            memoryCache.Remove(Constants.MARKETPLACE_ITEMS_CACHE_KEY);
        }

        public async Task CompleteAsync(int id)
        {
            var userEmail = httpContextAccessor.HttpContext.User.Claims.First(x => x.Value.Contains("vsgbg.com")).Value;
            string result = await repository.CompleteAsync(id);

            if (result != Constants.Ok)
            {
                throw new Exception(result);
            }

            memoryCache.Remove(Constants.PENDING_ORDERS_CACHE_KEY);
            memoryCache.Remove(Constants.MY_ORDERS_CACHE_KEY + userEmail);
        }

        public async Task DeleteAsync(int id)
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
            else
            {
                throw new Exception("Can't delete finished order!");
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
        }

        public async Task<IEnumerable<PendingOrderViewModel>> GetAllPendingOrdersAsync()
        {
            if (memoryCache.TryGetValue(Constants.PENDING_ORDERS_CACHE_KEY, out IEnumerable<PendingOrderViewModel> orders))
            {
                return orders;
            }

            var repositoryOrders = await repository.GetAllPendingOrdersAsync();
            List<PendingOrderViewModel> outputOrders = mapper.Map(repositoryOrders, new List<PendingOrderViewModel>());

            var options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                SlidingExpiration = TimeSpan.FromMinutes(2)
            };

            memoryCache.Set(Constants.PENDING_ORDERS_CACHE_KEY, outputOrders, options);

            return outputOrders;
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

            var repositoryOrders = await repository.GetByUserEmail(userEmail);
            List<MyOrdersViewModel> outputOrders = mapper.Map(repositoryOrders, new List<MyOrdersViewModel>());

            if (orders != null)
            {
                var options = new MemoryCacheEntryOptions()
                {
                    AbsoluteExpiration = DateTime.Now.AddMinutes(5),
                    SlidingExpiration = TimeSpan.FromMinutes(2)
                };

                memoryCache.Set(Constants.MY_ORDERS_CACHE_KEY + userEmail, outputOrders, options);
            }

            return outputOrders;
        }
    }
}
