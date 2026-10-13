using Ecommerce.Application.Interfaces.Infra;
using Microsoft.EntityFrameworkCore.Storage;

namespace Ecommerce.Infra.Data;

public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public async Task<ITransaction> BeginTransactionAsync()
    {
        var transaction = await dbContext.Database.BeginTransactionAsync();

        return new EfTransaction(transaction);
    }

    private sealed class EfTransaction(IDbContextTransaction transaction) : ITransaction
    {
        public Task CommitAsync() => transaction.CommitAsync();

        public Task RollbackAsync() => transaction.RollbackAsync();

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
