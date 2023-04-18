namespace VSGMarketplaceApi.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IOrderRepository Orders { get; }
    }
}
