using MediatR;

namespace ModulerERP.HR.Application.Features.CommissionRules.Queries;

public record CalculateCommissionQuery(Guid EmployeeId, decimal SalesAmount) : IRequest<decimal>;
