using ModulerERP.Sales.Application.DTOs;
using ModulerERP.SharedKernel.Results;

namespace ModulerERP.Sales.Application.Interfaces;

public interface IShipmentService
{
    Task<Result<List<ShipmentDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<ShipmentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAsync(CreateShipmentDto dto, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(Guid id, UpdateShipmentDto dto, CancellationToken cancellationToken = default);
    Task<Result> ShipAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> MarkDeliveredAsync(Guid id, CancellationToken cancellationToken = default);
}
