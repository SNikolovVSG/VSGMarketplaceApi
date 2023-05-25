using Data.Models;
using Data.ViewModels;

namespace Services.Interfaces
{
    public interface IOrdersService
    {
        public Task<string> BuyAsync(NewOrderAddModel input);

        public Task<string> CompleteAsync(int code);

        public Task<string> DeleteAsync(int code);

        public Task<IEnumerable<PendingOrderViewModel>> GetAllPendingOrdersAsync();

        public Task<Order> GetByCodeAsync(int code);

        public Task<IEnumerable<MyOrdersViewModel>> GetByUserEmail();

    }
}
