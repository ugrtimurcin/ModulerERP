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
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ProjectManagementDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProjectTaskService, ProjectTaskService>();
        services.AddScoped<IProjectTransactionService, ProjectTransactionService>();
        services.AddScoped<IProgressPaymentService, ProgressPaymentService>();
        services.AddScoped<IProjectDocumentService, ProjectDocumentService>();

        return services;
    }
}
