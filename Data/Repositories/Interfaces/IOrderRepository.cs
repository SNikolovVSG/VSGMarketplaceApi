using Data.Models;
using Data.ViewModels;

namespace Data.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetByCodeAsync(int code);
        Task<string> BuyAsync(Order order, Item item);
        Task<string> CompleteAsync(int code);
        Task<string> DeleteAsync(int code);  
        Task<IEnumerable<MyOrdersViewModel>> GetByUserEmail(string userEmail);
        Task<IEnumerable<PendingOrderViewModel>> GetAllPendingOrdersAsync();
        Task<string> RevertChangesFromPendingOrder(Order order);
    }
}
