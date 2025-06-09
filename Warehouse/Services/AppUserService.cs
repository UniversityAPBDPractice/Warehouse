using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Warehouse.Entities;
using Warehouse.Helpers;

namespace Warehouse.Services.Abstractions;

public class AppUserService : IAppUserService
{
    private string _connectionString;
    private string _secretKey;

    public AppUserService(IConfiguration cfg)
    {
        _connectionString = cfg.GetConnectionString("Default") ??
                            throw new ArgumentNullException(nameof(cfg), "No Default connection string was specified.");
        _secretKey = cfg["SecretKey"];

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

    public async Task<AppUser> LoginUserAsync(LoginRequest loginRequest)
    {
        const string query = @"
        SELECT Email, Password, Salt, RefreshToken, RefreshTokenExp
        FROM AppUsers
        WHERE Email = @Email;
    ";

        using (var con = new SqlConnection(_connectionString))
        using (var cmd = new SqlCommand(query, con))
        {
            cmd.Parameters.AddWithValue("@Email", loginRequest.Email);

            await con.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (!await reader.ReadAsync())
                {
                    return null; // User not found
                }

                var storedPassword = reader.GetString(reader.GetOrdinal("Password"));
                var storedSalt = reader.GetString(reader.GetOrdinal("Salt"));

                var hashedInput = SecurityHelpers.GetHashedPasswordWithSalt(loginRequest.Password, storedSalt);
                if (hashedInput != storedPassword)
                {
                    return null; // Invalid password
                }

                return new AppUser
                {
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    Password = storedPassword,
                    Salt = storedSalt,
                    RefreshToken = reader.GetString(reader.GetOrdinal("RefreshToken")),
                    RefreshTokenExp = reader.GetDateTime(reader.GetOrdinal("RefreshTokenExp"))
                };
            }
        }
    }

    public async Task<(string accessToken, string refreshToken)> RefreshUserAsync(RefreshRequest refreshToken)
    {
        const string selectQuery = @"
            SELECT Email, Password, Salt, RefreshToken, RefreshTokenExp
            FROM AppUsers
            WHERE RefreshToken = @RefreshToken;
        ";

        using var con = new SqlConnection(_connectionString);
        await con.OpenAsync();

        AppUser user = null;

        // Step 1: Read and buffer the user
        using (var cmd = new SqlCommand(selectQuery, con))
        {
            cmd.Parameters.AddWithValue("@RefreshToken", refreshToken.RefreshToken);
            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                throw new SecurityTokenException("Invalid refresh token");

            user = new AppUser
            {
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Password = reader.GetString(reader.GetOrdinal("Password")),
                Salt = reader.GetString(reader.GetOrdinal("Salt")),
                RefreshToken = reader.GetString(reader.GetOrdinal("RefreshToken")),
                RefreshTokenExp = reader.GetDateTime(reader.GetOrdinal("RefreshTokenExp"))
            };
        } // <- reader is now closed

        // Step 2: Validate token
        if (user.RefreshTokenExp < DateTime.UtcNow)
            throw new SecurityTokenException("Refresh token expired");

        // Step 3: Generate JWT
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, "user")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: "https://localhost:5001",
            audience: "https://localhost:5001",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: creds
        );

        var newRefreshToken = SecurityHelpers.GenerateRefreshToken();
        var newRefreshExp = DateTime.UtcNow.AddDays(1);

        // Step 4: Update the refresh token in DB
        const string updateQuery = @"
            UPDATE AppUsers
            SET RefreshToken = @NewRefreshToken,
                RefreshTokenExp = @NewRefreshExp
            WHERE Email = @Email;
        ";

        using (var updateCmd = new SqlCommand(updateQuery, con))
        {
            updateCmd.Parameters.AddWithValue("@NewRefreshToken", newRefreshToken);
            updateCmd.Parameters.AddWithValue("@NewRefreshExp", newRefreshExp);
            updateCmd.Parameters.AddWithValue("@Email", user.Email);
            await updateCmd.ExecuteNonQueryAsync();
        }

        return (
            accessToken: new JwtSecurityTokenHandler().WriteToken(jwtToken),
            refreshToken: newRefreshToken
        );
    }
}