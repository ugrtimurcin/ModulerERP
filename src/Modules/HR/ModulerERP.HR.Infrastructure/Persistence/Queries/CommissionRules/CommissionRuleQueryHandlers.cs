using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.CommissionRules.Queries;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Infrastructure.Persistence;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Infrastructure.Persistence.Queries.CommissionRules;

public class CommissionRuleQueryHandlers : 
    IRequestHandler<GetCommissionRulesQuery, IReadOnlyList<CommissionRuleDto>>,
    IRequestHandler<CalculateCommissionQuery, decimal>
{
    private readonly HRDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<Employee> _employeeRepository; // Using repository for simple ID lookup or Context

    public CommissionRuleQueryHandlers(
        HRDbContext context, 
        ICurrentUserService currentUserService,
        IRepository<Employee> employeeRepository)
    {
        _context = context;
        _currentUserService = currentUserService;
        _employeeRepository = employeeRepository;
    }

    public async Task<IReadOnlyList<CommissionRuleDto>> Handle(GetCommissionRulesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        
        return await _context.CommissionRules
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.Role).ThenBy(r => r.MinTargetAmount)
            .Select(r => new CommissionRuleDto(r.Id, r.Role, r.MinTargetAmount, r.Percentage, r.Basis))
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> Handle(CalculateCommissionQuery request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
        if (employee == null || string.IsNullOrEmpty(employee.JobTitle)) return 0;

        var tenantId = _currentUserService.TenantId;
        var rules = await _context.CommissionRules
            .Where(r => r.TenantId == tenantId && r.Role == employee.JobTitle)
            .OrderByDescending(r => r.MinTargetAmount) // Check highest targets first
            .ToListAsync(cancellationToken);
        
        // Find suitable rule: Highest MinTarget that is <= salesAmount
        var rule = rules.FirstOrDefault(r => request.SalesAmount >= r.MinTargetAmount);

        if (rule == null) return 0;

        // Apply percentage to the provided amount (assuming salesAmount matches the basis)
        return request.SalesAmount * (rule.Percentage / 100m);
    }
}
