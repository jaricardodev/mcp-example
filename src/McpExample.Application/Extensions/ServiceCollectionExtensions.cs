using McpExample.Application.Interfaces;
using McpExample.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace McpExample.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}
