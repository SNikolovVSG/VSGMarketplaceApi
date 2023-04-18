using VSGMarketplaceApi.Repositories.Interfaces;

namespace VSGMarketplaceApi.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(IOrderRepository orderRepository, IItemRepository items)
        {
            Orders = orderRepository;
            Items = items;
        }

        public IOrderRepository Orders { get; }

        public IItemRepository Items { get; }
    }
}
