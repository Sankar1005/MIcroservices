using System.Data;
using Microsoft.Data.SqlClient;
using ProductService.Api.Models;

namespace ProductService.Api.Repositories;

/// <summary>
/// ADO.NET implementation of <see cref="IProductRepository"/>.
/// Every value sent to SQL Server goes through a SqlParameter, never through string concatenation.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public ProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MicroservicesDb")
            ?? throw new InvalidOperationException("Connection string 'MicroservicesDb' is not configured.");
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Name, Description, Price, StockQuantity, IsActive, CreatedAt, UpdatedAt
            FROM product.Products
            WHERE IsActive = 1
            ORDER BY Id;
            """;

        var products = new List<Product>();

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            products.Add(MapProduct(reader));
        }

        return products;
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, Name, Description, Price, StockQuantity, IsActive, CreatedAt, UpdatedAt
            FROM product.Products
            WHERE Id = @Id AND IsActive = 1;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? MapProduct(reader) : null;
    }

    public async Task<int> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        // OUTPUT INSERTED.Id returns the new identity value in the same round trip.
        // IsActive and CreatedAt use the column defaults (1 and SYSUTCDATETIME()).
        const string sql = """
            INSERT INTO product.Products (Name, Description, Price, StockQuantity)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Description, @Price, @StockQuantity);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        AddProductParameters(command, product);

        await connection.OpenAsync(cancellationToken);
        var newId = await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(newId);
    }

    public async Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE product.Products
            SET Name = @Name,
                Description = @Description,
                Price = @Price,
                StockQuantity = @StockQuantity,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND IsActive = 1;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        AddProductParameters(command, product);
        command.Parameters.Add(new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = (object?)product.UpdatedAt ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = product.Id });

        await connection.OpenAsync(cancellationToken);
        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE product.Products
            SET IsActive = 0,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id AND IsActive = 1;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

        await connection.OpenAsync(cancellationToken);
        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

        return rowsAffected > 0;
    }

    private static void AddProductParameters(SqlCommand command, Product product)
    {
        command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = product.Name });
        command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, 1000) { Value = (object?)product.Description ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@Price", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = product.Price });
        command.Parameters.Add(new SqlParameter("@StockQuantity", SqlDbType.Int) { Value = product.StockQuantity });
    }

    private static Product MapProduct(SqlDataReader reader)
    {
        return new Product
        {
            Id = (int)reader["Id"],
            Name = (string)reader["Name"],
            Description = reader["Description"] as string,   // DBNull becomes null
            Price = (decimal)reader["Price"],
            StockQuantity = (int)reader["StockQuantity"],
            IsActive = (bool)reader["IsActive"],
            // The database stores UTC (SYSUTCDATETIME), so mark the values as UTC.
            CreatedAt = DateTime.SpecifyKind((DateTime)reader["CreatedAt"], DateTimeKind.Utc),
            UpdatedAt = reader["UpdatedAt"] is DateTime updatedAt
                ? DateTime.SpecifyKind(updatedAt, DateTimeKind.Utc)
                : null
        };
    }
}
