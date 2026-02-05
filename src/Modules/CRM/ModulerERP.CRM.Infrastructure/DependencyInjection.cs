using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.CRM.Infrastructure.Services;

namespace ModulerERP.CRM.Infrastructure;

/// <summary>
/// Extension methods for registering CRM module services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCRMInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<CRMDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(CRMDbContext).Assembly.FullName)));

        // Register services
        services.AddScoped<IBusinessPartnerService, BusinessPartnerService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<ILeadService, LeadService>();

        services.AddScoped<IOpportunityService, OpportunityService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ITicketMessageService, TicketMessageService>();
        services.AddScoped<ISupportTicketService, SupportTicketService>();

        return services;
    }
}
