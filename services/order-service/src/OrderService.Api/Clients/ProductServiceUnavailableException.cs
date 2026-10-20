namespace OrderService.Api.Clients;

/// <summary>
/// ProductService is down, too slow, or returned a server error (even after retries).
/// </summary>
public class ProductServiceUnavailableException : Exception
{
    public ProductServiceUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
