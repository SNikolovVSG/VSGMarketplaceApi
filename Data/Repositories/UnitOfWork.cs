using Data.Repositories.Interfaces;

namespace Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(IOrderRepository orderRepository, IItemRepository items, IImageRepository images)
        {
            Orders = orderRepository;
            Items = items;
            Images = images;
        }

        public IOrderRepository Orders { get; }

        public IItemRepository Items { get; }

        public IImageRepository Images { get; }
    }
}
