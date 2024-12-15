using webapi.Data;
using webapi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL connection
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("idenitycs")));

// Configure Identity with API endpoints and role management
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>();

// Dependency injection for role service
builder.Services.AddScoped<IRoleService, RoleService>();

// Configure Swagger/OpenAPI with security definition
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Description = "Enter 'Bearer {your token}'"
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

// Add CORS policy for allowing all origins, methods, and headers
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin() // Add your GitHub Pages URL
                   .AllowAnyMethod()
                   .AllowAnyHeader();
    });
});

// MongoDB service for weather-related operations
builder.Services.AddSingleton<MongoWeatherService>();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Map Identity API endpoints
app.MapIdentityApi<IdentityUser>();

// Use CORS policy
app.UseCors("AllowAll");

// Enable Swagger in all environments, with appropriate URL prefix for production
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Weather API v1");
        options.RoutePrefix = "swagger"; // Access Swagger at /swagger
    });
}

// Middleware for HTTPS redirection
app.UseHttpsRedirection();

// Middleware for authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes
app.MapControllers();

// Run the application
app.Run();
