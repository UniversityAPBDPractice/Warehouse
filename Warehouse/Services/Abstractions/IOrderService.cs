namespace Warehouse.Services.Abstractions;

public interface IOrderService
{
    Task<int> GetOrderIdWithProductAsync(int idProduct, int amount, DateTime earlierThan, CancellationToken token);
    Task<bool> OrderCompletedAsync(int idOrder, CancellationToken token);
    Task<int> FulfillAsync(int idOrder, CancellationToken token);
}