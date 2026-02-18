using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Payroll.Queries;

public record GetPayrollSummaryQuery(int Year, int Month) : IRequest<PayrollSummaryDto>;
