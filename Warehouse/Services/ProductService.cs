using Microsoft.Data.SqlClient;
using Warehouse.Services.Abstractions;

namespace Warehouse.Services;

public class ProductService : IProductService
{
    private string _connectionString;
    public ProductService(IConfiguration cfg)
    {
        _connectionString = cfg.GetConnectionString("Default") ??
                                    throw new ArgumentNullException(nameof(cfg), "No Default connection string was specified.");
    }
    public async Task<int> GetPriceByIdAsync(int id, CancellationToken token)
    {
        const string query = "SELECT Price FROM Product WHERE IdProduct = @idProduct";
        using (SqlConnection con = new SqlConnection(_connectionString))
        using (SqlCommand com = new SqlCommand(query, con))
        {
            await con.OpenAsync(token);
            com.Parameters.AddWithValue("@idProduct", id);

            var price = Convert.ToInt32(await com.ExecuteScalarAsync(token));
            return price;
        }
    }

    public async Task<bool> ProductExistsByIdAsync(int id, CancellationToken token)
    {
        const string query = "SELECT COUNT(*) \nFROM Product \nWHERE IdProduct = @idProduct";
        using (SqlConnection con = new SqlConnection(_connectionString))
        using (SqlCommand com = new SqlCommand(query, con))
        {
            await con.OpenAsync(token);
            com.Parameters.AddWithValue("@idProduct", id);

            var count = Convert.ToInt32(await com.ExecuteScalarAsync(token));
            return count > 0;
        }
    }
}