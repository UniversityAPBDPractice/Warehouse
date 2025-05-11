using Warehouse.Entities;
using Warehouse.Services.Abstractions;

namespace Warehouse.Services;

public static class ServicesDefaultImplementations
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IProductWarehouseService, ProductWarehouseService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IWarehouseService, WarehouseService>();

        return services;
    }
}