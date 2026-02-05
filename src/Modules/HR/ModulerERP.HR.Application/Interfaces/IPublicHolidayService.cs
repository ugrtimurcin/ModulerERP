using ModulerERP.HR.Application.DTOs;

namespace ModulerERP.HR.Application.Interfaces;

public interface IPublicHolidayService
{
    Task<IReadOnlyList<PublicHolidayDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Guid> CreateAsync(CreatePublicHolidayDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
