using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Payroll.Queries;

public record GetPayrollsQuery(int Year) : IRequest<List<PayrollDto>>;
