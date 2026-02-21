using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModulerERP.CRM.Application;
using ModulerERP.CRM.Application.Interfaces;
using ModulerERP.CRM.Domain.Entities;
using ModulerERP.CRM.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.CRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCRMInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CRMDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(CRMDbContext).Assembly.FullName)));

        // Unit of Work
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<CRMDbContext>());
        services.AddScoped<ICRMUnitOfWork>(provider => provider.GetRequiredService<CRMDbContext>());

        // Register Repositories for all CRM entities
        services.AddScoped<IRepository<Lead>, CRMRepository<Lead>>();
        services.AddScoped<IRepository<Opportunity>, CRMRepository<Opportunity>>();
        services.AddScoped<IRepository<BusinessPartner>, CRMRepository<BusinessPartner>>();
        services.AddScoped<IRepository<Contact>, CRMRepository<Contact>>();
        services.AddScoped<IRepository<Activity>, CRMRepository<Activity>>();
        services.AddScoped<IRepository<SupportTicket>, CRMRepository<SupportTicket>>();
        services.AddScoped<IRepository<TicketMessage>, CRMRepository<TicketMessage>>();
        services.AddScoped<IRepository<Tag>, CRMRepository<Tag>>();
        // EntityTag and PartnerBalance don't inherit BaseEntity — use DbSet directly
        services.AddScoped<IRepository<SaleAgent>, CRMRepository<SaleAgent>>();
        services.AddScoped<IRepository<BusinessPartnerGroup>, CRMRepository<BusinessPartnerGroup>>();


        // Add Application Services (MediatR + FluentValidation)
        services.AddCRMApplication();

        // Register MediatR Handlers from Infrastructure (for query handlers that need DbContext)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        return services;
    }
}
