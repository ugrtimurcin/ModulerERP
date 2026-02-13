using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ModulerERP.SystemCore.Application.Interfaces;
using ModulerERP.SharedKernel.Interfaces; // Added for IExchangeRateService
using ModulerERP.SystemCore.Infrastructure.Persistence;
using ModulerERP.SystemCore.Infrastructure.Services;

namespace ModulerERP.SystemCore.Infrastructure;

/// <summary>
/// Dependency injection extensions for SystemCore module.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddSystemCoreInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // DbContext
        // DbContext
        services.AddDbContext<SystemCoreDbContext>(options =>
             options.UseNpgsql(
                 configuration.GetConnectionString("DefaultConnection"),
                 b => {
                     b.MigrationsAssembly(typeof(SystemCoreDbContext).Assembly.FullName);
                     b.MigrationsHistoryTable("__EFMigrationsHistory", "system_core");
                 }));

        // JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Configure JWT Authentication
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        // Register Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddScoped<ICurrencyLookupService, CurrencyLookupService>();

        return services;
    }
}
