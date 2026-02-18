using MediatR;
using ModulerERP.HR.Application.DTOs;
using ModulerERP.HR.Application.Features.WorkShifts.Commands;
using ModulerERP.HR.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.HR.Application.Features.WorkShifts.Commands;

public class WorkShiftCommandHandlers :
    IRequestHandler<CreateWorkShiftCommand, WorkShiftDto>,
    IRequestHandler<UpdateWorkShiftCommand>,
    IRequestHandler<DeleteWorkShiftCommand>
{
    private readonly IRepository<WorkShift> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public WorkShiftCommandHandlers(
        IRepository<WorkShift> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<WorkShiftDto> Handle(CreateWorkShiftCommand request, CancellationToken cancellationToken)
    {
        // Parse "HH:mm" to TimeSpan
        if (!TimeSpan.TryParse(request.StartTime, out var start) || !TimeSpan.TryParse(request.EndTime, out var end))
        {
            throw new ArgumentException("Invalid time format. Use HH:mm");
        }

        var entity = WorkShift.Create(
            _currentUserService.TenantId,
            _currentUserService.UserId,
            request.Name,
            start,
            end,
            request.BreakMinutes
        );

        await _repository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task Handle(UpdateWorkShiftCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null) throw new KeyNotFoundException($"WorkShift {request.Id} not found");

        if (!TimeSpan.TryParse(request.StartTime, out var start) || !TimeSpan.TryParse(request.EndTime, out var end))
        {
            throw new ArgumentException("Invalid time format. Use HH:mm");
        }

        entity.Update(request.Name, start, end, request.BreakMinutes);
        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(DeleteWorkShiftCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity != null)
        {
            _repository.Remove(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private static WorkShiftDto ToDto(WorkShift e) => new(
        e.Id,
        e.Name,
        e.StartTime.ToString(@"hh\:mm"),
        e.EndTime.ToString(@"hh\:mm"),
        e.BreakMinutes
    );
}
