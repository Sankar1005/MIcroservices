using Microservices.Shared.Logging;
using OrderService.Api.Clients;
using OrderService.Api.Repositories;
using OrderService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

const string AngularCorsPolicy = "AllowAngularApp";

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// From shared/Microservices.Shared.Logging: global exception handler (clean 500, no stack trace)
// and one log line per request without personal data.
builder.Services.AddSharedLogging();

// Dependency injection: OrdersController -> IOrderService -> IOrderRepository (own database)
//                                                         -> IProductServiceClient (ProductService over HTTP)
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
// Fully qualified because the class has the same name as the project's root namespace.
builder.Services.AddScoped<IOrderService, OrderService.Api.Services.OrderService>();

// Service-to-service communication: a typed HttpClient for ProductService.
// IHttpClientFactory creates and reuses the underlying connections; the base address comes from
// appsettings.json ("Services:ProductService:BaseUrl"), so it can change per environment without code changes.
var productServiceBaseUrl = builder.Configuration["Services:ProductService:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration 'Services:ProductService:BaseUrl' is missing.");

builder.Services
    .AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
    {
        client.BaseAddress = new Uri(productServiceBaseUrl);
    })
    // Resilience: what happens when ProductService is slow or down.
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 2;                                 // try up to 3 times in total
        options.Retry.Delay = TimeSpan.FromMilliseconds(200);               // short wait between tries
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(5);           // give up on one try after 5 s
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(15);     // give up completely after 15 s
        // Circuit breaker (defaults): after many failures, stop calling ProductService for a while
        // and fail immediately, so a broken ProductService does not slow OrderService down too.
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Must be first so it sees every request and catches exceptions thrown anywhere later in the pipeline.
app.UseSharedLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "OrderService v1"));
}
else
{
    // Not used in Development so the Angular app can call http://localhost:5102 without being redirected to HTTPS.
    app.UseHttpsRedirection();
}

app.UseCors(AngularCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
