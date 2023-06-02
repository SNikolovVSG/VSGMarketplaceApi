using Data.Models;
using Data.ViewModels;

namespace Data.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(int id);
        Task<string> BuyAsync(Order order, Item item);
        Task<string> CompleteAsync(int id);
        Task<string> DeleteAsync(int id);  
        Task<IEnumerable<Order>> GetByUserEmail(string userEmail);
        Task<IEnumerable<Order>> GetAllPendingOrdersAsync();
        Task<string> RevertChangesFromPendingOrder(Order order);
    }
}
