using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.PostingProfiles.Commands;

public record CreatePostingProfileCommand(CreatePostingProfileDto Dto, Guid CreatedByUserId) : IRequest<Result<PostingProfileDto>>;

public class CreatePostingProfileCommandHandler : IRequestHandler<CreatePostingProfileCommand, Result<PostingProfileDto>>
{
    private readonly IRepository<PostingProfile> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreatePostingProfileCommandHandler(
        IRepository<PostingProfile> repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PostingProfileDto>> Handle(CreatePostingProfileCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var pType = (TransactionType)dto.TransactionType;

        var profile = PostingProfile.Create(
            _currentUserService.TenantId,
            $"{pType}-{Guid.NewGuid().ToString()[..4]}", // Auto-generated Code Since FE doesn't send it
            $"{pType} Profile", // Auto-generated Name
            pType,
            request.CreatedByUserId,
            dto.Category,
            dto.IsDefault);

        if (dto.AccountId != Guid.Empty)
        {
            // Simple approach: arbitrarily use AccountsReceivable or Revenue.
            // It just maps a TransactionType -> AccountId for the FE
            profile.AddLine(PostingAccountRole.Revenue, dto.AccountId);
        }

        await _repository.AddAsync(profile, cancellationToken);
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
