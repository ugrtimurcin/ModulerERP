using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;
using ModulerERP.ProjectManagement.Infrastructure.Services;

namespace ModulerERP.ProjectManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectManagementInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
            services.AddDbContext<ProjectManagementDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ProjectManagementDbContext).Assembly.FullName)));

        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectTaskService, ProjectTaskService>();
        services.AddScoped<IProjectTransactionService, ProjectTransactionService>();
        services.AddScoped<IProgressPaymentService, ProgressPaymentService>();
        services.AddScoped<IProjectDocumentService, ProjectDocumentService>();
        services.AddScoped<IProjectChangeOrderService, ProjectChangeOrderService>();
        services.AddScoped<IProjectFinancialService, ProjectFinancialService>();
        services.AddScoped<IDailyLogService, DailyLogService>();
        services.AddScoped<IProjectResourceService, ProjectResourceService>();
        services.AddScoped<IResourceRateCardService, ResourceRateCardService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
