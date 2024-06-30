using WarehouseWebApi.Models;
namespace WarehouseWebApi.Services;

public interface IWarehouseService
{
    Task<bool> WarehouseExists(int id);
    Task<bool> ProductExists(int id);
    Task<int> OrderExists(ProductWarehouse warehouse);
    Task FulfilledAtUpdate(Order order);
    Task<int> Product_WarehouseInsert(ProductWarehouse productWarehouse, Order order, Product product);
}