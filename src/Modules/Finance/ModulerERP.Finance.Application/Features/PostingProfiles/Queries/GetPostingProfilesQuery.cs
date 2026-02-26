using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace ModulerERP.Finance.Application.Features.PostingProfiles.Queries;

public record GetPostingProfilesQuery : IRequest<Result<List<PostingProfileDto>>>;

public class GetPostingProfilesQueryHandler : IRequestHandler<GetPostingProfilesQuery, Result<List<PostingProfileDto>>>
{
    private readonly IRepository<PostingProfile> _repository;

    public GetPostingProfilesQueryHandler(IRepository<PostingProfile> repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<PostingProfileDto>>> Handle(GetPostingProfilesQuery request, CancellationToken cancellationToken)
    {
        var profiles = await _repository.GetAllAsync(cancellationToken);

        var dtos = profiles.Select(p => new PostingProfileDto
        {
            Id = p.Id,
            Code = p.Code,
            Name = p.Name,
            TransactionType = (int)p.TransactionType,
            Category = p.Category,
            IsDefault = p.IsDefault,
            AccountId = p.Lines.FirstOrDefault()?.AccountId
        }).ToList();

        return Result<List<PostingProfileDto>>.Success(dtos);
    }
}
