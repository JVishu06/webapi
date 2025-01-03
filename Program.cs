using webapi.Data;
using webapi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Configure Database
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("idenitycs")));

// Configure Identity
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>();

// Add Custom Services
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddSingleton<MongoWeatherService>();

// Add Controllers
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = OpenApiParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://wasm-ilwk.onrender.com") // Replace with actual frontend URL
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure Middleware
if (app.Environment.IsDevelopment())
{
    // Enable Swagger for development
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Configure for production
    app.UseHttpsRedirection(); // Ensure HTTPS is enforced
}

// Enable CORS
app.UseCors("AllowSpecificOrigins");

// Configure Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map Endpoints
app.MapIdentityApi<IdentityUser>();
app.MapControllers();

app.Run();
