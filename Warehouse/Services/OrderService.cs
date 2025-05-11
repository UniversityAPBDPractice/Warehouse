using Warehouse.Services.Abstractions;

namespace Warehouse.Services;

using Microsoft.Data.SqlClient;

public class OrderService: IOrderService
{
    private string _connectionString;
    public OrderService(IConfiguration cfg)
    {
        _connectionString = cfg.GetConnectionString("Default") ??
                            throw new ArgumentNullException(nameof(cfg), "No Default connection string was specified.");
    }
    public async Task<int> GetOrderIdWithProductAsync(int idProduct, int amount, DateTime earlierThan, CancellationToken token)
    {
        const string query = "SELECT IdOrder FROM [Order] WHERE IdProduct = @idProduct AND [Order].CreatedAt < @earlierThan";
        using (SqlConnection con = new SqlConnection(_connectionString))
        using (SqlCommand com = new SqlCommand(query, con))
        {
            await con.OpenAsync(token);
            com.Parameters.AddWithValue("@idProduct", idProduct);
            com.Parameters.AddWithValue("@earlierThan", earlierThan);

            var result = await com.ExecuteScalarAsync(token);
            return result is not DBNull ? Convert.ToInt32(result) : -1;
        }
    }

    public async Task<bool> OrderCompletedAsync(int idOrder, CancellationToken token)
    {
        const string query = "SELECT FulfilledAt FROM [Order] WHERE IdOrder = @idOrder";
        using (SqlConnection con = new SqlConnection(_connectionString))
        using (SqlCommand com = new SqlCommand(query, con))
        {
            await con.OpenAsync(token);
            com.Parameters.AddWithValue("@idOrder", idOrder);

            var result = await com.ExecuteScalarAsync(token);
            return result is not DBNull;
        }
    }

    public async Task<int> FulfillAsync(int idOrder, CancellationToken token)
    {
        const string query = "UPDATE [Order] SET FulfilledAt = @timeNow WHERE IdOrder = @idOrder";
        using (SqlConnection con = new SqlConnection(_connectionString))
        using (SqlCommand com = new SqlCommand(query, con))
        {
            await con.OpenAsync(token);
            com.Parameters.AddWithValue("@idOrder", idOrder);
            com.Parameters.AddWithValue("@timeNow", DateTime.Now);

            var rowsAffected = await com.ExecuteNonQueryAsync(token);
            return rowsAffected;
        }
    }
}