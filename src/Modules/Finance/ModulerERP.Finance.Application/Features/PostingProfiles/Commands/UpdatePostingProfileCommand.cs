using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.PostingProfiles.Commands;

public record UpdatePostingProfileCommand(Guid Id, UpdatePostingProfileDto Dto) : IRequest<Result<PostingProfileDto>>;

public class UpdatePostingProfileCommandHandler : IRequestHandler<UpdatePostingProfileCommand, Result<PostingProfileDto>>
{
    private readonly IRepository<PostingProfile> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePostingProfileCommandHandler(
        IRepository<PostingProfile> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PostingProfileDto>> Handle(UpdatePostingProfileCommand request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (profile == null)
            return Result<PostingProfileDto>.Failure("Posting Profile not found.");

        var dto = request.Dto;
        profile.Update(profile.Name, dto.Category, dto.IsDefault);
        
        // Very basic line replacement for the simplified UI
        profile.Lines.Clear();
        if (dto.AccountId != Guid.Empty)
        {
            profile.AddLine(PostingAccountRole.Revenue, dto.AccountId);
        }

        _repository.Update(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var resultDto = new PostingProfileDto
        {
            Id = profile.Id,
            Code = profile.Code,
            Name = profile.Name,
            TransactionType = (int)profile.TransactionType,
            Category = profile.Category,
            IsDefault = profile.IsDefault,
            AccountId = dto.AccountId
        };

        return Result<PostingProfileDto>.Success(resultDto);
    }
}
