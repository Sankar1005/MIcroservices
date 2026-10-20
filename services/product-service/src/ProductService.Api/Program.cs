using Microservices.Shared.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using ProductService.Api.Repositories;
using ProductService.Api.Services;

var builder = WebApplication.CreateBuilder(args);

const string AngularCorsPolicy = "AllowAngularApp";

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// From shared/Microservices.Shared.Logging: global exception handler (clean 500, no stack trace)
// and one log line per request without personal data.
builder.Services.AddSharedLogging();

// Dependency injection: ProductsController -> IProductService -> IProductRepository.
builder.Services.AddScoped<IProductRepository, ProductRepository>();
// Fully qualified because the class has the same name as the project's root namespace.
builder.Services.AddScoped<IProductService, ProductService.Api.Services.ProductService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Requests from clients arrive through the API Gateway, which adds X-Forwarded-Host/-Proto/-For.
// Reading them makes links (e.g. the Location header of 201 Created) use the gateway's public address
// and logs show the real client. By default only proxies on this machine (localhost) are trusted.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost);

var app = builder.Build();

// Before everything else, so the rest of the pipeline sees the original (public) host and scheme.
app.UseForwardedHeaders();

// Must be next so it sees every request and catches exceptions thrown anywhere later in the pipeline.
app.UseSharedLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "ProductService v1"));
}
else
{
    // Not used in Development so the Angular app and OrderService can call http://localhost:5101
    // without being redirected to HTTPS.
    app.UseHttpsRedirection();
}

app.UseCors(AngularCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
