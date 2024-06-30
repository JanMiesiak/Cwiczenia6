using Microsoft.Data.SqlClient;
using WarehouseWebApi.Models;

namespace WarehouseWebApi.Services;


    public class WarehouseService : IWarehouseService
    {
        private readonly IConfiguration _configuration;

        public WarehouseService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> ProductExists(int id)
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", id);

            return await command.ExecuteScalarAsync() != null;
        }

        public async Task<bool> WarehouseExists(int id)
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
            command.Parameters.AddWithValue("@IdWarehouse", id);

            return await command.ExecuteScalarAsync() != null;
        }

        public async Task<int> OrderExists(ProductWarehouse productWarehouse)
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT IdOrder 
                FROM [Order] 
                WHERE IdProduct = @IdProduct 
                  AND Amount = @Amount 
                  AND FulfilledAt IS NULL 
                  AND CreatedAt < @CreatedAt";

            command.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
            command.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
            command.Parameters.AddWithValue("@CreatedAt", productWarehouse.CreatedAt);

            var result = await command.ExecuteScalarAsync();
            return result == null ? -1 : (int)result;
        }

        public async Task FulfilledAtUpdate(Order order)
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE [Order] SET FulfilledAt = @FulfilledAt WHERE IdOrder = @IdOrder";

            command.Parameters.AddWithValue("@FulfilledAt", DateTime.UtcNow);
            command.Parameters.AddWithValue("@IdOrder", order.IdOrder);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<decimal> PriceGet(Product product)
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
            command.Parameters.AddWithValue("@IdProduct", product.IdProduct);

            var result = await command.ExecuteScalarAsync();
            if (result == null)
            {
                throw new Exception("Price not found.");
            }

            return (decimal)result;
        }

        public async Task<int> Product_WarehouseInsert(ProductWarehouse productWarehouse, Order order, Product product)
        {
            await using var connection = new SqlConnection(_configuration.GetConnectionString("Default"));
            await connection.OpenAsync();

            decimal price = await PriceGet(product);

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt) 
                VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt);
                SELECT SCOPE_IDENTITY();";

            command.Parameters.AddWithValue("@IdWarehouse", productWarehouse.IdWarehouse);
            command.Parameters.AddWithValue("@IdProduct", productWarehouse.IdProduct);
            command.Parameters.AddWithValue("@IdOrder", order.IdOrder);
            command.Parameters.AddWithValue("@Amount", productWarehouse.Amount);
            command.Parameters.AddWithValue("@Price", productWarehouse.Amount * price);
            command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

            var primaryKey = await command.ExecuteScalarAsync();
            return Convert.ToInt32(primaryKey);
        }
    }
