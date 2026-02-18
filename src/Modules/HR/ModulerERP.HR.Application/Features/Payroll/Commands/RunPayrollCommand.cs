using MediatR;
using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Features.Payroll.Commands;

public record RunPayrollCommand(RunPayrollDto Dto) : IRequest<PayrollDto>;
