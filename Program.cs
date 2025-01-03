using webapi.Data;
using webapi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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

// Configure Swagger/OpenAPI for JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = OpenApiParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://wasm-ilwk.onrender.com", "https://localhost:7195")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Enable credentials if needed
    });
});

// Configure Authentication for JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-identity-server-url";  // Update with your Identity URL
        options.Audience = "api1";  // Specify your API's audience
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
