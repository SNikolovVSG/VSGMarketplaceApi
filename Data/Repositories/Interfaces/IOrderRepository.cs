using Data.Models;
using Data.ViewModels;

namespace Data.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetByCodeAsync(int code);
        Task<string> BuyAsync(NewOrderAddModel entity);
        Task<string> CompleteAsync(int code);
        Task<string> DeleteAsync(int code, string userEmail);  
        Task<IEnumerable<MyOrdersViewModel>> GetByUserEmail(string userEmail);
        Task<IEnumerable<PendingOrderViewModel>> GetAllPendingOrdersAsync();
    }
}
