using MediatR;
using ModulerERP.HR.Application.Features.PublicHolidays.Commands;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.PublicHolidays.Commands;

public class PublicHolidayCommandHandlers :
    IRequestHandler<CreatePublicHolidayCommand, Guid>,
    IRequestHandler<DeletePublicHolidayCommand>
{
    private readonly IRepository<PublicHoliday> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public PublicHolidayCommandHandlers(
        IRepository<PublicHoliday> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreatePublicHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = PublicHoliday.Create(
            _currentUserService.TenantId, 
            _currentUserService.UserId, 
            request.Date, 
            request.Name, 
            request.IsHalfDay
        );

        await _repository.AddAsync(holiday, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return holiday.Id;
    }

    public async Task Handle(DeletePublicHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (holiday != null)
        {
            _repository.Remove(holiday);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
