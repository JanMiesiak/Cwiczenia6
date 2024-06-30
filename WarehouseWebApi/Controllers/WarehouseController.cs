using WarehouseWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using WarehouseWebApi.Services;
namespace WarehouseWebApi.Controllers;


[Route("api/warehouses")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IConfiguration configuration, IWarehouseService warehouseService)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _warehouseService = warehouseService ?? throw new ArgumentNullException(nameof(warehouseService));
    }

    [HttpPost]
    public async Task<IActionResult> AddProductWarehouse(ProductWarehouse newProductWarehouse)
    {
        if (newProductWarehouse == null)
        {
            return BadRequest("Request body cannot be empty.");
        }

        try
        {
           
            if (!await _warehouseService.ProductExists(newProductWarehouse.IdProduct))
            {
                return NotFound($"Product with IdProduct = {newProductWarehouse.IdProduct} not found.");
            }

            
            if (!await _warehouseService.WarehouseExists(newProductWarehouse.IdWarehouse))
            {
                return NotFound($"Warehouse with IdWarehouse = {newProductWarehouse.IdWarehouse} not found.");
            }

            
            int idOrder = await _warehouseService.OrderExists(newProductWarehouse);
            if (idOrder == -1)
            {
                return NotFound("No matching order found.");
            }

           
            Product product = new Product { IdProduct = newProductWarehouse.IdProduct };
            Order order = new Order
            {
                IdOrder = idOrder,
                IdProduct = newProductWarehouse.IdProduct,
                Amount = newProductWarehouse.Amount,
                CreatedAt = newProductWarehouse.CreatedAt
            };

            
            await _warehouseService.FulfilledAtUpdate(order);

           
            var primaryKey = await _warehouseService.Product_WarehouseInsert(newProductWarehouse, order, product);

            
            return Ok(primaryKey);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
        }
    }
}