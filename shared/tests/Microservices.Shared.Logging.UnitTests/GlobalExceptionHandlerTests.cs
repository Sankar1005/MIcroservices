using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Microservices.Shared.Logging.UnitTests;

public class GlobalExceptionHandlerTests
{
    private readonly FakeLogger<GlobalExceptionHandler> _logger = new();
    private readonly DefaultHttpContext _httpContext = new();

    public GlobalExceptionHandlerTests()
    {
        _httpContext.Request.Method = "GET";
        _httpContext.Request.Path = "/api/products/1";
        _httpContext.Request.QueryString = new QueryString("?email=someone@example.com");
        _httpContext.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task Returns_500_with_a_generic_message_and_no_exception_details()
    {
        var handler = new GlobalExceptionHandler(_logger);

        var handled = await handler.TryHandleAsync(
            _httpContext, new InvalidOperationException("Login failed for user 'sa'"), TestContext.Current.CancellationToken);

        var body = await ReadResponseBodyAsync();
        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, _httpContext.Response.StatusCode);
        Assert.Equal("application/problem+json", _httpContext.Response.ContentType);
        Assert.Contains("An unexpected error occurred.", body);
        Assert.Contains("traceId", body);
        Assert.DoesNotContain("Login failed", body);
        Assert.DoesNotContain("InvalidOperationException", body);
    }

    [Fact]
    public async Task Logs_the_exception_with_method_and_path_but_not_the_query_string()
    {
        var handler = new GlobalExceptionHandler(_logger);
        var exception = new InvalidOperationException("boom");

        await handler.TryHandleAsync(_httpContext, exception, TestContext.Current.CancellationToken);

        var record = Assert.Single(_logger.Collector.GetSnapshot());
        Assert.Equal(LogLevel.Error, record.Level);
        Assert.Same(exception, record.Exception);
        Assert.Contains("GET /api/products/1", record.Message);
        Assert.DoesNotContain("someone@example.com", record.Message);
    }

    private async Task<string> ReadResponseBodyAsync()
    {
        _httpContext.Response.Body.Position = 0;
        using var reader = new StreamReader(_httpContext.Response.Body);
        return await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
    }
}
