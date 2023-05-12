using VSGMarketplaceApi.Data.Models;
using VSGMarketplaceApi.ViewModels;

namespace VSGMarketplaceApi.Data.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetByCodeAsync(int code);
        Task<string> AddAsync(NewOrderAddModel entity);
        Task<string> CompleteAsync(int code);
        Task<string> DeleteAsync(int code, int userId);
        Task<IEnumerable<MyOrdersViewModel>> GetByUserId(int userId);
        Task<IEnumerable<PendingOrderViewModel>> GetAllPendingOrdersAsync();
    }
}
