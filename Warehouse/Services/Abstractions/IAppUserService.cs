using Microsoft.AspNetCore.Identity.Data;

namespace Warehouse.Services.Abstractions;

public interface IAppUserService
{
    Task<int> RegisterUserAsync(RegisterRequest model);
}