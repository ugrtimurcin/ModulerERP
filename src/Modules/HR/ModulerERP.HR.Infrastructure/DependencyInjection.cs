using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModulerERP.HR.Application;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddHRInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HRDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(HRDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<HRDbContext>());
        services.AddScoped<IHRUnitOfWork>(provider => provider.GetRequiredService<HRDbContext>());

        // Register Repositories
        services.AddScoped<IRepository<Employee>, HRRepository<Employee>>();
        services.AddScoped<IRepository<Department>, HRRepository<Department>>();
        services.AddScoped<IRepository<SalaryHistory>, HRRepository<SalaryHistory>>();
        services.AddScoped<IRepository<WorkShift>, HRRepository<WorkShift>>();
        services.AddScoped<IRepository<AttendanceLog>, HRRepository<AttendanceLog>>();
        services.AddScoped<IRepository<DailyAttendance>, HRRepository<DailyAttendance>>();
        services.AddScoped<IRepository<LeaveRequest>, HRRepository<LeaveRequest>>();
        services.AddScoped<IRepository<Payroll>, HRRepository<Payroll>>();
        services.AddScoped<IRepository<PayrollEntry>, HRRepository<PayrollEntry>>();
        services.AddScoped<IRepository<AdvanceRequest>, HRRepository<AdvanceRequest>>();
        services.AddScoped<IRepository<PublicHoliday>, HRRepository<PublicHoliday>>();
        services.AddScoped<IRepository<CommissionRule>, HRRepository<CommissionRule>>();
        services.AddScoped<IRepository<PeriodCommission>, HRRepository<PeriodCommission>>();
        services.AddScoped<IRepository<Bonus>, HRRepository<Bonus>>();
        services.AddScoped<IRepository<TaxRule>, HRRepository<TaxRule>>();
        services.AddScoped<IRepository<SocialSecurityRule>, HRRepository<SocialSecurityRule>>();
        services.AddScoped<IRepository<MinimumWage>, HRRepository<MinimumWage>>();
        services.AddScoped<IRepository<PayrollParameter>, HRRepository<PayrollParameter>>();
        
        // Add Application Services
        services.AddHRApplication();

        // Register MediatR Handlers from Infrastructure (Queries)
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        return services;
    }
}
