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

// Register Identity services
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>();

builder.Services.AddScoped<IRoleService, RoleService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger for API documentation and security
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    option.OperationFilter<SecurityRequirementsOperationFilter>();
});

// Temporarily allow all origins for debugging CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()   // Allow any origin for now
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Register MongoWeatherService
builder.Services.AddSingleton<MongoWeatherService>();

// Build the application
var app = builder.Build();

// Use Identity API endpoints
app.MapIdentityApi<IdentityUser>();

// Use CORS policy
app.UseCors("AllowAll");

// Use Swagger in Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable HTTPS Redirection
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Add a simple health check endpoint
app.MapGet("/api/HealthCheck", () => Results.Ok("API is running"));

// Map controllers for handling HTTP requests
app.MapControllers();

// Run the app
app.Run();
