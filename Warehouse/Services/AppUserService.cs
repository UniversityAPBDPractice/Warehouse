using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Data.SqlClient;
using Warehouse.Entities;
using Warehouse.Helpers;

namespace Warehouse.Services.Abstractions;

public class AppUserService : IAppUserService
{
    private string _connectionString;
    public AppUserService(IConfiguration cfg)
    {
        _connectionString = cfg.GetConnectionString("Default") ??
                            throw new ArgumentNullException(nameof(cfg), "No Default connection string was specified.");
    }
    public async Task<int> RegisterUserAsync(RegisterRequest model)
    {
        var hashedPasswordAndSalt = SecurityHelpers.GetHashedPasswordAndSalt(model.Password);
        
        string query = @"
        INSERT INTO AppUsers (Email, Password, Salt, RefreshToken, RefreshTokenExp)
        VALUES (@Email, @Password, @Salt, @RefreshToken, @RefreshTokenExp);
    ";
        using (SqlConnection con = new SqlConnection(_connectionString))
        using (SqlCommand com = new SqlCommand(query, con))
        {
            var parameters = new
            {
                Email = model.Email,
                Password = hashedPasswordAndSalt.Item1,
                Salt = hashedPasswordAndSalt.Item2,
                RefreshToken = SecurityHelpers.GenerateRefreshToken(),
                RefreshTokenExp = DateTime.UtcNow.AddDays(1)
            };
            
            await con.OpenAsync();
            com.Parameters.AddWithValue("@Email", parameters.Email);
            com.Parameters.AddWithValue("@Password", parameters.Password);
            com.Parameters.AddWithValue("@Salt", parameters.Salt);
            com.Parameters.AddWithValue("@RefreshToken", parameters.RefreshToken);
            com.Parameters.AddWithValue("@RefreshTokenExp", parameters.RefreshTokenExp);

            var rowsAffected = await com.ExecuteNonQueryAsync();
            return rowsAffected;
        }
    }
}