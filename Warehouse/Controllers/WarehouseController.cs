using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Warehouse.Entities;
using Warehouse.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using Warehouse.Helpers;

namespace Warehouse.Controllers;

[ApiController]
[Route("/api/warehouse")]
public class WarehouseController : ControllerBase
{
    private IProductService _productService;
    private IWarehouseService _warehouseService;
    private IOrderService _orderService;
    private IProductWarehouseService _productWarehouseService;
    private IAppUserService _appUserService;
    public WarehouseController(
        IProductService productService,
        IWarehouseService warehouseService,
        IOrderService orderService,
        IProductWarehouseService productWarehouseService,
        IAppUserService appUserService)
    {
        _productService = productService;
        _warehouseService = warehouseService;
        _orderService = orderService;
        _productWarehouseService = productWarehouseService;
        _appUserService = appUserService;
    }
    
    [Authorize]
    [HttpPost]
    [Route("{idProduct:int}/{idWarehouse:int}/{amount:int}")]
    public async Task<IActionResult> CreateProductWarehouseAsync(
        [FromRoute] int idProduct,
        [FromRoute] int idWarehouse,
        [FromRoute] int amount,
        CancellationToken token)
    {
        if (!await _productService.ProductExistsByIdAsync(idProduct, token)) return NotFound();
        if (!await _warehouseService.WarehouseExistsByIdAsync(idWarehouse, token)) return NotFound();
        if (!(amount > 0)) return StatusCode(405);
        var idOrder = await _orderService.GetOrderIdWithProductAsync(idProduct, amount, DateTime.Now, token);
        if (idOrder == -1) return StatusCode(405);
        // TODO check if order has been completed
        await _orderService.FulfillAsync(idOrder, token);

        var idProductWarehouse = await _productWarehouseService.GetNewIdAsync(token);
        var productPrice = await _productService.GetPriceByIdAsync(idProduct, token);
        ProductWarehouse newProductWarehouse = new ProductWarehouse
        {
            IdProductWarehouse = idProductWarehouse,
            IdWarehouse = idWarehouse,
            IdProduct = idProduct,
            IdOrder = idOrder,
            Amount = amount,
            Price = amount * productPrice,
            CreatedAt = DateTime.Now
        };

        await _productWarehouseService.CreateProductWarehouseAsync(newProductWarehouse, token);
        return Ok();
    }

    [Authorize]
    [HttpPost]
    [Route("/procedure/{idProduct:int}/{idWarehouse:int}/{amount:int}")]
    public async Task<IActionResult> CreateProductWarehouseProcedureAsync(
        [FromRoute] int idProduct,
        [FromRoute] int idWarehouse,
        [FromRoute] int amount,
        CancellationToken token)
    {
        try
        {
            await _productWarehouseService.CreateProductWarehouseProcedureAsync(idProduct, idWarehouse, amount, token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
            return StatusCode(400);
        }

        return Ok();
    }
    
    [HttpGet("test-error")]
    public IActionResult ThrowError()
    {
        throw new Exception("Test exception from controller");
    }
    
    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult RegisterUserAsync(RegisterRequest model)
    {
        _appUserService.RegisterUserAsync(model);
        return Ok();
    }
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        var user = await _appUserService.LoginUserAsync(loginRequest);

        if (user == null)
            return Unauthorized();

        // Generate tokens, etc.
        return Ok(user);
    }
}