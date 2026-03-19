using McpExample.Domain.Interfaces;
using McpExample.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace McpExample.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        return services;
    }
}
