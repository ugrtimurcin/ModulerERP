using ModulerERP.Finance.Application.DTOs;
using ModulerERP.SharedKernel.Results;
using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Application.Interfaces;

public interface IFiscalPeriodService
{
    Task<Result<List<FiscalPeriodDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<FiscalPeriodDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<FiscalPeriodDto>> CreateAsync(CreateFiscalPeriodDto dto, Guid createdByUserId, CancellationToken cancellationToken = default);
    Task<Result<FiscalPeriodDto>> UpdateAsync(Guid id, UpdateFiscalPeriodDto dto, CancellationToken cancellationToken = default);
    Task<Result> GeneratePeriodsAsync(int year, Guid createdByUserId, CancellationToken cancellationToken = default); // Helper to gen 12 months
}
