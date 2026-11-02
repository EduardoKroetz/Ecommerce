namespace Ecommerce.Application.Interfaces.Infra;

public interface IUnitOfWork
{
    Task SaveChangesAsync();
    Task<ITransaction> BeginTransactionAsync();
}

public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}