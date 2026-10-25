namespace Ecommerce.Application.Extensions;

public static class TimeProviderExtensions
{
    /// <summary>
    /// Current UTC date and time as a <see cref="DateTime"/>, the type used by the entities.
    /// </summary>
    public static DateTime GetUtcDateTime(this TimeProvider timeProvider) => timeProvider.GetUtcNow().UtcDateTime;
}
