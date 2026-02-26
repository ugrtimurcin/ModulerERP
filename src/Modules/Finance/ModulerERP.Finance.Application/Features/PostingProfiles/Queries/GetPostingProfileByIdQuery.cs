using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace ModulerERP.Finance.Application.Features.PostingProfiles.Queries;

public record GetPostingProfileByIdQuery(Guid Id) : IRequest<Result<PostingProfileDto>>;

public class GetPostingProfileByIdQueryHandler : IRequestHandler<GetPostingProfileByIdQuery, Result<PostingProfileDto>>
{
    private readonly IRepository<PostingProfile> _repository;

    public GetPostingProfileByIdQueryHandler(IRepository<PostingProfile> repository)
    {
        _repository = repository;
    }

    public async Task<Result<PostingProfileDto>> Handle(GetPostingProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (profile == null)
            return Result<PostingProfileDto>.Failure("Posting Profile not found.");

        var dto = new PostingProfileDto
        {
            Id = profile.Id,
            Code = profile.Code,
            Name = profile.Name,
            TransactionType = (int)profile.TransactionType,
            Category = profile.Category,
            IsDefault = profile.IsDefault,
            AccountId = profile.Lines.FirstOrDefault()?.AccountId
        };

        return Result<PostingProfileDto>.Success(dto);
    }
}
