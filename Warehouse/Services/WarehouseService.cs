using Microsoft.Data.SqlClient;
using Warehouse.Services.Abstractions;

namespace Warehouse.Services;

public class WarehouseService : IWarehouseService
{
    private string _connectionString;
    public WarehouseService(IConfiguration cfg)
    {
        _connectionString = cfg.GetConnectionString("Default") ??
                            throw new ArgumentNullException(nameof(cfg), "No Default connection string was specified.");
    }

    public async Task<bool> WarehouseExistsByIdAsync(int id, CancellationToken token)
    {
        const string query = "SELECT COUNT(*) FROM Warehouse";
        using (SqlConnection con = new SqlConnection(_connectionString))
        using (SqlCommand com = new SqlCommand(query, con))
        {
            await con.OpenAsync(token);

            var result = await com.ExecuteScalarAsync(token);
            return result is not DBNull;
        }
    }
}