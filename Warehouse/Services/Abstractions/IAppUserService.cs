using Microsoft.AspNetCore.Identity.Data;
using Warehouse.Entities;

namespace Warehouse.Services.Abstractions;

public interface IAppUserService
{
    Task<int> RegisterUserAsync(RegisterRequest model);
    Task<AppUser> LoginUserAsync(LoginRequest model);
}