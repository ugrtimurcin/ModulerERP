using MediatR;

namespace ModulerERP.CRM.Application.Features.Leads.Commands;

public record DeleteLeadCommand(Guid Id) : IRequest;

public class DeleteLeadCommandHandler : IRequestHandler<DeleteLeadCommand>
{
    private readonly SharedKernel.Interfaces.IRepository<Domain.Entities.Lead> _leadRepository;
    private readonly Interfaces.ICRMUnitOfWork _unitOfWork;
    private readonly SharedKernel.Interfaces.ICurrentUserService _currentUserService;

    public DeleteLeadCommandHandler(
        SharedKernel.Interfaces.IRepository<Domain.Entities.Lead> leadRepository,
        Interfaces.ICRMUnitOfWork unitOfWork,
        SharedKernel.Interfaces.ICurrentUserService currentUserService)
    {
        _leadRepository = leadRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Lead with Id '{request.Id}' not found.");

        lead.Delete(_currentUserService.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
