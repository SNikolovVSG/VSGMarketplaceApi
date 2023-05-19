namespace Data.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IOrderRepository Orders { get; }

        IItemRepository Items { get; }

        IImageRepository Images { get; }
    }
}
