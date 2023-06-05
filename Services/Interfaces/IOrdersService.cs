using Data.Models;
using Data.ViewModels;

namespace Services.Interfaces
{
    public interface IOrdersService
    {
        public Task BuyAsync(NewOrderAddModel input);

        public Task CompleteAsync(int code);

        public Task DeleteAsync(int code);

        public Task<IEnumerable<PendingOrderViewModel>> GetAllPendingOrdersAsync();

        public Task<Order> GetByIdAsync(int code);

        public Task<IEnumerable<MyOrdersViewModel>> GetByUserEmail();

    }
}
