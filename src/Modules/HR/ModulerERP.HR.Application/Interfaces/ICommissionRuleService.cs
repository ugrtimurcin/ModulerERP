using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Interfaces;

public interface ICommissionRuleService
{
    Task<IReadOnlyList<CommissionRuleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreateCommissionRuleDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<decimal> CalculateCommissionAsync(Guid employeeId, decimal salesAmount, CancellationToken cancellationToken = default);
}
