using webapi.Data;
using webapi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Use a secure connection string for production (store it in environment variables or a secrets manager)
builder.Services.AddDbContext<IdentityDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("idenitycs")));

builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>();

builder.Services.AddScoped<IRoleService, RoleService>();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = OpenApiParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    option.OperationFilter<SecurityRequirementsOperationFilter>();
});

// Define a CORS policy to allow specific origins in production
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", builder =>
    {
        builder.WithOrigins("https://wasm-ilwk.onrender.com") // Replace with actual production frontend URL
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.AddSingleton<MongoWeatherService>();

builder.Services.AddControllers();

var app = builder.Build();

// Map identity API endpoints
app.MapIdentityApi<IdentityUser>();

// Enable CORS policy in production
app.UseCors("AllowSpecificOrigins");

if (app.Environment.IsDevelopment())
{
    // Enable Swagger only in development
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Disable Swagger UI in production
    // Configure HTTPS redirection and authentication
    app.UseHttpsRedirection(); // Ensure that the server handles HTTPS in production
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
