using MediatR;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Inventory.Application.Features.Inventory.Commands;

public record ReceiveGoodsCommand(Guid TransferId) : IRequest<Result<Guid>>;
