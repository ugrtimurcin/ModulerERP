using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Payroll.Queries;

public record GetPayrollByIdQuery(Guid Id) : IRequest<PayrollDto?>;
