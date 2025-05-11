using Microsoft.Data.SqlClient;
using Warehouse.Entities;
using Warehouse.Services.Abstractions;

namespace Warehouse.Services;

public class ProductWarehouseService : IProductWarehouseService
{
    private string _connectionString;
    public ProductWarehouseService(IConfiguration cfg)
    {
        _connectionString = cfg.GetConnectionString("Default") ??
                            throw new ArgumentNullException(nameof(cfg), "No Default connection string was specified.");
    }
    public async Task<int> CreateProductWarehouseAsync(ProductWarehouse pw, CancellationToken token)
    {
        const string query = """
                             INSERT INTO Product_Warehouse 
                             VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)
                             """;
        using (SqlConnection con = new SqlConnection(_connectionString))
        using (SqlCommand com = new SqlCommand(query, con))
        {
            await con.OpenAsync(token);
            com.Parameters.AddWithValue("@IdWarehouse", pw.IdWarehouse);
            com.Parameters.AddWithValue("@IdProduct", pw.IdProduct);
            com.Parameters.AddWithValue("@IdOrder", pw.IdOrder);
            com.Parameters.AddWithValue("@Amount", pw.Amount);
            com.Parameters.AddWithValue("@Price", pw.Price);
            com.Parameters.AddWithValue("@CreatedAt", pw.CreatedAt);

            var rowsAffected = await com.ExecuteNonQueryAsync(token);
            return rowsAffected;
        }
    }

    public async Task<int> GetNewIdAsync(CancellationToken token)
    {
        const string query = "SELECT MAX(IdProductWarehouse) FROM Product_Warehouse";
        using (SqlConnection con = new SqlConnection(_connectionString))
        using (SqlCommand com = new SqlCommand(query, con))
        {
            await con.OpenAsync(token);

            var result = await com.ExecuteScalarAsync(token);
            var maxId = result is DBNull ? 1 : Convert.ToInt32(result);
            return maxId + 1;
        }
    }
}