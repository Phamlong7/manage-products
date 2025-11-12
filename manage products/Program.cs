using ManageProducts.Application.Common.Interfaces;
using ManageProducts.Application.Products;
using ManageProducts.Infrastructure.Persistence;
using ManageProducts.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressInferBindingSourcesForParameters = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Product Inventory API",
        Version = "v1",
        Description = "RESTful API for managing product inventory with CRUD operations, search, filtering, and sorting capabilities.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "API Support"
        }
    });
});

builder.Services.AddDbContext<ProductDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("ProductDatabase");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Connection string 'ProductDatabase' is not configured.");
    }

    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

var allowedOrigins = builder.Configuration.GetSection("ClientApp:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendClient", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Location");
    });
});

var app = builder.Build();

// Enable Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Inventory API v1");
    options.RoutePrefix = "swagger"; // Swagger UI will be available at /swagger
    options.DisplayRequestDuration();
});

app.UseCors("FrontendClient");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Run database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure port from Render's PORT environment variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://+:{port}");

app.Run();
