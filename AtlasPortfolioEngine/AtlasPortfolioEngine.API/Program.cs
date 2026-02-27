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
    // Combine multiple versioning strategies (URL segment and header)
    // We support both simultaneously using ApiVersionReader.
    // Combine so consumers can choose their preferred style.
    options.ApiVersionReader =  ApiVersionReader.Combine(
        // URL Versioning — /api/v1/clients 
        // URL versioning is explicit and cacheable — great for public APIs.
        new UrlSegmentApiVersionReader(), 

        // Header Versioning — api-version: 1 in request headers
        // Header versioning keeps URLs clean
        // Header versioning allows for more complex versioning strategies.
        // Header versioning preferred in enterprise APIs like Azure
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

builder.Services.AddAuthentication( JwtBearerDefaults.AuthenticationScheme)
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

// EF Core
builder.Services.AddDbContext<AtlasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application Services
builder.Services.AddScoped<IRiskProfileService, RiskProfileService>();
builder.Services.AddScoped<IModelPortfolioService, ModelPortfolioService>();
builder.Services.AddScoped<IDriftDetectionService, DriftDetectionService>();
builder.Services.AddScoped<IRebalancingService, RebalancingService>();
builder.Services.AddScoped<ISuitabilityService, SuitabilityService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AtlasPortfolioEngine.API v1");
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "AtlasPortfolioEngine.API v2");
        }
    );
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AtlasDbContext>();
    await AtlasDbContextSeed.SeedAsync(context);
}

app.Run();