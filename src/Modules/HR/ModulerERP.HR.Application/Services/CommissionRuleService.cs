using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Interfaces;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.HR.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Services;

public class CommissionRuleService : ICommissionRuleService
{
    private readonly IRepository<CommissionRule> _repository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CommissionRuleService(IRepository<CommissionRule> repository, IRepository<Employee> employeeRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<CommissionRuleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _repository.GetAllAsync(cancellationToken);
        return rules.Select(r => new CommissionRuleDto(r.Id, r.Role, r.MinTargetAmount, r.Percentage, r.Basis)).ToList();
    }

    public async Task<Guid> CreateAsync(CreateCommissionRuleDto dto, CancellationToken cancellationToken = default)
    {
        var rule = CommissionRule.Create(_currentUserService.TenantId, _currentUserService.UserId, dto.Role, dto.MinTargetAmount, dto.Percentage, dto.Basis);
        await _repository.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return rule.Id;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var rule = await _repository.GetByIdAsync(id, cancellationToken);
        if (rule != null)
        {
            _repository.Remove(rule);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<decimal> CalculateCommissionAsync(Guid employeeId, decimal salesAmount, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null || string.IsNullOrEmpty(employee.JobTitle)) return 0;

        var rules = await _repository.FindAsync(r => r.Role == employee.JobTitle, cancellationToken);
        
        // Find suitable rule: Highest MinTarget that is <= salesAmount
        var rule = rules.Where(r => salesAmount >= r.MinTargetAmount)
                        .OrderByDescending(r => r.MinTargetAmount)
                        .FirstOrDefault();

        if (rule == null) return 0;

        // Apply percentage to the provided amount (assuming salesAmount matches the basis)
        return salesAmount * (rule.Percentage / 100m);
        
    }
}
