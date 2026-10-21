namespace Ecommerce.Infra.Services;

/// <summary>
/// Ambient cancellation for repository reads (ADR 0013). Writes are never cancelled.
/// </summary>
public interface ICancellationTokenProvider
{
    CancellationToken Token { get; }
}
