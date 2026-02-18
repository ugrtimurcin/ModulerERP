using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Payroll.Queries;

public record GetPayrollEntriesQuery(Guid PayrollId) : IRequest<List<PayrollEntryDto>>;
