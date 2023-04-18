using VSGMarketplaceApi.Repositories.Interfaces;

namespace VSGMarketplaceApi.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(IOrderRepository orderRepository)
        {
            Orders = orderRepository;
        }

        public IOrderRepository Orders { get; }
    }
}
