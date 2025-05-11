namespace Warehouse.Services.Abstractions;

public interface IWarehouseService
{
    Task<bool> WarehouseExistsByIdAsync(int id, CancellationToken token);
}