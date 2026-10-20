using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;

namespace Microservices.Shared.Logging;

public static class SharedLoggingExtensions
{
    /// <summary>
    /// Registers the global exception handler and request logging.
    /// Call in Program.cs: <c>builder.Services.AddSharedLogging();</c>
    /// </summary>
    public static IServiceCollection AddSharedLogging(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddHttpLogging(options =>
        {
            // One log line per request with method, path, status code and duration.
            // Never headers, query strings or bodies: they can contain personal data.
            options.LoggingFields = HttpLoggingFields.RequestMethod
                                  | HttpLoggingFields.RequestPath
                                  | HttpLoggingFields.ResponseStatusCode
                                  | HttpLoggingFields.Duration;
            options.CombineLogs = true;
        });

        return services;
    }

    /// <summary>
    /// Adds the middleware. Call it first in the pipeline so it sees every request and every exception:
    /// <c>app.UseSharedLogging();</c>
    /// </summary>
    public static IApplicationBuilder UseSharedLogging(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        app.UseHttpLogging();
        return app;
    }
}
