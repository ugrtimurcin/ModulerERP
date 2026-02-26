using MediatR;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.PostingProfiles.Commands;

public record DeletePostingProfileCommand(Guid Id) : IRequest<Result<Guid>>;

public class DeletePostingProfileCommandHandler : IRequestHandler<DeletePostingProfileCommand, Result<Guid>>
{
    private readonly IRepository<PostingProfile> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePostingProfileCommandHandler(
        IRepository<PostingProfile> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(DeletePostingProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (profile == null)
            return Result<Guid>.Failure("Posting Profile not found.");

        _repository.Remove(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(request.Id);
    }
}
