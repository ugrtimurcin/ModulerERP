using MediatR;
using ModulerERP.SharedKernel.Results;
using ModulerERP.Inventory.Application.DTOs;

namespace ModulerERP.Inventory.Application.Features.Inventory.Commands;

public record AdjustStockCommand(CreateStockMovementDto Dto) : IRequest<Result<Guid>>;
