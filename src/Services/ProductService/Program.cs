using ProductService.Repositories;
using ProductService.Services;

var builder = WebApplication.CreateBuilder(args);

const string AngularCorsPolicy = "AllowAngularApp";

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Global error handling: turns unhandled exceptions into a clean JSON error (ProblemDetails), never a stack trace.
builder.Services.AddProblemDetails();

// Dependency injection: ProductsController -> IProductService -> IProductRepository.
builder.Services.AddScoped<IProductRepository, ProductRepository>();
// Fully qualified because the class has the same name as the project's root namespace.
builder.Services.AddScoped<IProductService, ProductService.Services.ProductService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Must be first so it catches exceptions thrown anywhere later in the pipeline.
// The exception is logged; the client only gets a 500 with a generic message.
app.UseExceptionHandler();

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
