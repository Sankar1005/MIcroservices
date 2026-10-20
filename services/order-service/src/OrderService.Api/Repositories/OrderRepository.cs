using System.Data;
using Microsoft.Data.SqlClient;
using OrderService.Api.Models;

namespace OrderService.Api.Repositories;

/// <summary>
/// ADO.NET implementation of <see cref="IOrderRepository"/>.
/// Every value sent to SQL Server goes through a SqlParameter, never through string concatenation.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public OrderRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MicroservicesDb")
            ?? throw new InvalidOperationException("Connection string 'MicroservicesDb' is not configured.");
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // Two SELECTs in one round trip: first the orders, then all their items.
        const string sql = """
            SELECT Id, CustomerName, TotalAmount, IsActive, CreatedAt, UpdatedAt
            FROM ordering.Orders
            WHERE IsActive = 1
            ORDER BY Id;

            SELECT i.Id, i.OrderId, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity
            FROM ordering.OrderItems AS i
            INNER JOIN ordering.Orders AS o ON o.Id = i.OrderId
            WHERE o.IsActive = 1
            ORDER BY i.OrderId, i.Id;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await ReadOrdersWithItemsAsync(reader, cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, CustomerName, TotalAmount, IsActive, CreatedAt, UpdatedAt
            FROM ordering.Orders
            WHERE Id = @Id AND IsActive = 1;

            SELECT i.Id, i.OrderId, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity
            FROM ordering.OrderItems AS i
            INNER JOIN ordering.Orders AS o ON o.Id = i.OrderId
            WHERE i.OrderId = @Id AND o.IsActive = 1
            ORDER BY i.Id;
            """;

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

        await connection.OpenAsync(cancellationToken);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var orders = await ReadOrdersWithItemsAsync(reader, cancellationToken);
        return orders.Count == 0 ? null : orders[0];
    }

    public async Task<int> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        const string insertOrderSql = """
            INSERT INTO ordering.Orders (CustomerName, TotalAmount)
            OUTPUT INSERTED.Id
            VALUES (@CustomerName, @TotalAmount);
            """;

        const string insertItemSql = """
            INSERT INTO ordering.OrderItems (OrderId, ProductId, ProductName, UnitPrice, Quantity)
            VALUES (@OrderId, @ProductId, @ProductName, @UnitPrice, @Quantity);
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        // All or nothing: if any INSERT fails, the transaction is rolled back when it is disposed
        // without Commit, so there is never an order without its items.
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        int orderId;
        await using (var orderCommand = new SqlCommand(insertOrderSql, connection, transaction))
        {
            orderCommand.Parameters.Add(new SqlParameter("@CustomerName", SqlDbType.NVarChar, 200) { Value = order.CustomerName });
            orderCommand.Parameters.Add(new SqlParameter("@TotalAmount", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = order.TotalAmount });

            orderId = Convert.ToInt32(await orderCommand.ExecuteScalarAsync(cancellationToken));
        }

        foreach (var item in order.Items)
        {
            await using var itemCommand = new SqlCommand(insertItemSql, connection, transaction);
            itemCommand.Parameters.Add(new SqlParameter("@OrderId", SqlDbType.Int) { Value = orderId });
            itemCommand.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = item.ProductId });
            itemCommand.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.NVarChar, 200) { Value = item.ProductName });
            itemCommand.Parameters.Add(new SqlParameter("@UnitPrice", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = item.UnitPrice });
            itemCommand.Parameters.Add(new SqlParameter("@Quantity", SqlDbType.Int) { Value = item.Quantity });

            await itemCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return orderId;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE ordering.Orders
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

    /// <summary>
    /// Reads the first result set (orders) and then the second (items), and puts each item under its order.
    /// </summary>
    private static async Task<IReadOnlyList<Order>> ReadOrdersWithItemsAsync(SqlDataReader reader, CancellationToken cancellationToken)
    {
        var orders = new List<Order>();
        var ordersById = new Dictionary<int, Order>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var order = new Order
            {
                Id = (int)reader["Id"],
                CustomerName = (string)reader["CustomerName"],
                TotalAmount = (decimal)reader["TotalAmount"],
                IsActive = (bool)reader["IsActive"],
                // The database stores UTC (SYSUTCDATETIME), so mark the values as UTC.
                CreatedAt = DateTime.SpecifyKind((DateTime)reader["CreatedAt"], DateTimeKind.Utc),
                UpdatedAt = reader["UpdatedAt"] is DateTime updatedAt
                    ? DateTime.SpecifyKind(updatedAt, DateTimeKind.Utc)
                    : null
            };
            orders.Add(order);
            ordersById[order.Id] = order;
        }

        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var item = new OrderItem
            {
                Id = (int)reader["Id"],
                OrderId = (int)reader["OrderId"],
                ProductId = (int)reader["ProductId"],
                ProductName = (string)reader["ProductName"],
                UnitPrice = (decimal)reader["UnitPrice"],
                Quantity = (int)reader["Quantity"]
            };

            if (ordersById.TryGetValue(item.OrderId, out var order))
            {
                order.Items.Add(item);
            }
        }

        return orders;
    }
}
