using Warehouse.Entities;

namespace Warehouse.Services.Abstractions;

public interface IProductWarehouseService
{
    Task<int> CreateProductWarehouseAsync(ProductWarehouse pw, CancellationToken token);
    Task<int> GetNewIdAsync(CancellationToken token);
}