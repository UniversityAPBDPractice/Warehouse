using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Warehouse.Entities;
using Warehouse.Services.Abstractions;

namespace Warehouse.Controllers;

[ApiController]
[Route("/api/warehouse")]
public class WarehouseController : ControllerBase
{
    private IProductService _productService;
    private IWarehouseService _warehouseService;
    private IOrderService _orderService;
    private IProductWarehouseService _productWarehouseService;
    public WarehouseController(
        IProductService productService,
        IWarehouseService warehouseService,
        IOrderService orderService,
        IProductWarehouseService productWarehouseService)
    {
        _productService = productService;
        _warehouseService = warehouseService;
        _orderService = orderService;
        _productWarehouseService = productWarehouseService;
    }
    
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
}