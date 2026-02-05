using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Interfaces;

public interface IPayrollService
{
    Task<IEnumerable<PayrollDto>> GetByYearAsync(int year, CancellationToken cancellationToken = default);
    Task<PayrollDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PayrollEntryDto>> GetEntriesAsync(Guid payrollId, CancellationToken cancellationToken = default);
    Task<PayrollDto> RunPayrollAsync(RunPayrollDto dto, CancellationToken cancellationToken = default);
    Task<PayrollSummaryDto> GetSummaryAsync(int year, int month, CancellationToken cancellationToken = default);
}
