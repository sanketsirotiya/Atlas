using AtlasPortfolioEngine.Application.Interfaces;
using AtlasPortfolioEngine.Application.Services;
using AtlasPortfolioEngine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("api-version")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AtlasPortfolioEngine.API", Version = "v1" });
    c.SwaggerDoc("v2", new() { Title = "AtlasPortfolioEngine.API", Version = "v2" });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// EF Core — Railway injects DATABASE_URL automatically; fall back to appsettings for local dev
var connectionString =
    Environment.GetEnvironmentVariable("DATABASE_URL") ??
    Environment.GetEnvironmentVariable("DATABASE_PUBLIC_URL") ??
    builder.Configuration.GetConnectionString("DefaultConnection")!;

if (connectionString.StartsWith("Host=", StringComparison.OrdinalIgnoreCase) ||
    connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
    connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<AtlasDbContext>(options => options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<AtlasDbContext>(options => options.UseSqlServer(connectionString));
}

// Application Services
builder.Services.AddScoped<IRiskProfileService, RiskProfileService>();
builder.Services.AddScoped<IModelPortfolioService, ModelPortfolioService>();
builder.Services.AddScoped<IDriftDetectionService, DriftDetectionService>();
builder.Services.AddScoped<IRebalancingService, RebalancingService>();
builder.Services.AddScoped<ISuitabilityService, SuitabilityService>();

// CORS — allow localhost (dev), local network (mobile), and any *.onrender.com origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.SetIsOriginAllowed(origin =>
        {
            var uri = new Uri(origin);
            return uri.Host == "localhost" ||
                   uri.Host.StartsWith("192.168.") ||
                   uri.Host.EndsWith(".onrender.com") ||
                   uri.Host.EndsWith(".railway.app") ||
                   uri.Host.EndsWith(".up.railway.app");
        })
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Swagger available in all environments for demo purposes
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AtlasPortfolioEngine.API v1");
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "AtlasPortfolioEngine.API v2");
});

// app.UseHttpsRedirection(); // Render handles HTTPS at the reverse proxy level

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Create schema and seed data on startup (works for both fresh PostgreSQL and existing SQL Server)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AtlasDbContext>();
    await context.Database.EnsureCreatedAsync();
    await AtlasDbContextSeed.SeedAsync(context);
}

// Bind to PORT env var set by Render, fallback to 5146 for local dev
var port = Environment.GetEnvironmentVariable("PORT") ?? "5146";
app.Run($"http://0.0.0.0:{port}");
