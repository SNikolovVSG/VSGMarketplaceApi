namespace VSGMarketplaceApi.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IOrderRepository Orders { get; }

        IItemRepository Items { get; }
    }
}
