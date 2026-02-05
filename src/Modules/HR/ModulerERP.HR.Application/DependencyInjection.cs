using Microsoft.Extensions.DependencyInjection;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Application.Services;

namespace ModulerERP.HR.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddHRApplication(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IWorkShiftService, WorkShiftService>();
        services.AddScoped<IAdvanceRequestService, AdvanceRequestService>();
        services.AddScoped<IPublicHolidayService, PublicHolidayService>();
        services.AddScoped<ICommissionRuleService, CommissionRuleService>();
        services.AddScoped<IAttendanceLogService, AttendanceLogService>();
        
        return services;
    }
}

