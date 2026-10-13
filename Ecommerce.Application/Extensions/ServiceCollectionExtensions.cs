using Ecommerce.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registers every concrete class in the Ecommerce.Application.Services namespace as scoped.
        // IsNested filters out compiler-generated types (async state machines, closures).
        var servicesNamespace = typeof(AuthService).Namespace;

        var serviceTypes = typeof(ServiceCollectionExtensions).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsNested: false } && t.Namespace == servicesNamespace);

        foreach (var serviceType in serviceTypes)
        {
            services.AddScoped(serviceType);
        }

        return services;
    }
}
