using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ecommerce.IntegrationTests.Fixtures;

// Runs a concurrent request at the first write point of the next operation: it commits between the service's
// reads and its writes, the same window as two real requests (ADR 0021)
public sealed class ConcurrentRequestInterceptor : IDbTransactionInterceptor, ISaveChangesInterceptor
{
    private Func<Task>? _pendingRequest;

    public bool HasPendingRequest => _pendingRequest is not null;

    public void RunBeforeNextWrite(Func<Task> concurrentRequest) => _pendingRequest = concurrentRequest;

    public void Reset() => _pendingRequest = null;

    // At the transaction start, not at SaveChanges: inside the transaction the service locks rows before saving
    // (e.g. IncrementStockAsync), so a concurrent request would wait for those locks while the service waits for it,
    // and the test would hang until the command timeout. Depends on services reading everything before opening the
    // transaction (Conventions.md).
    public async ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(
        DbConnection connection,
        TransactionStartingEventData eventData,
        InterceptionResult<DbTransaction> result,
        CancellationToken cancellationToken = default)
    {
        await RunPendingRequestAsync();

        return result;
    }

    // Operations without a transaction (e.g. PayAsync) write for the first time at SaveChanges
    public async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await RunPendingRequestAsync();

        return result;
    }

    // Cleared before running, so the concurrent request's own writes don't run it again
    private async Task RunPendingRequestAsync()
    {
        var request = _pendingRequest;

        if (request is null)
            return;

        _pendingRequest = null;

        await request();
    }
}
