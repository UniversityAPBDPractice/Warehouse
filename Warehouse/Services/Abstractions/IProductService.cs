namespace Warehouse.Services.Abstractions;

public interface IProductService
{
    Task<bool> ProductExistsByIdAsync(int id, CancellationToken token);
    Task<int> GetPriceByIdAsync(int id, CancellationToken token);
}