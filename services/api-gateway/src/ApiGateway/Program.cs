using Microservices.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

const string AngularCorsPolicy = "AllowAngularApp";

// From shared/Microservices.Shared.Logging: global exception handler (clean 500, no stack trace)
// and one log line per request without personal data.
builder.Services.AddSharedLogging();

// YARP reverse proxy. All routes and destinations live in appsettings.json ("ReverseProxy"),
// so a new service only needs a config change here, never a code change.
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// The gateway is the only address the browser talks to, so CORS is configured here
// (the Origin header is removed before requests are forwarded to the services).
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// GET /health: is the gateway itself running? (Not the services behind it.)
builder.Services.AddHealthChecks();

var app = builder.Build();

// Must be first so it sees every request and catches exceptions thrown anywhere later in the pipeline.
app.UseSharedLogging();

if (!app.Environment.IsDevelopment())
{
    // Not used in Development so the Angular app can call http://localhost:5100 without being redirected to HTTPS.
    app.UseHttpsRedirection();
}

// Answers CORS preflight (OPTIONS) requests itself; they are never forwarded.
app.UseCors(AngularCorsPolicy);

app.MapGet("/", () => Results.Ok(new
{
    service = "ApiGateway",
    routes = new[] { "/api/products/** -> ProductService", "/api/orders/** -> OrderService" },
    health = "/health"
}));

app.MapHealthChecks("/health");

// Every request that matches a route in appsettings.json is forwarded to its service.
app.MapReverseProxy();

app.Run();
