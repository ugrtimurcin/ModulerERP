using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Features.PostingProfiles.Commands;
using ModulerERP.Finance.Application.Features.PostingProfiles.Queries;
using MediatR;

namespace ModulerERP.Api.Controllers.Finance;

[ApiController]
[Route("api/finance/posting-profiles")]
public class PostingProfilesController : BaseApiController
{
    private readonly ISender _sender;

    public PostingProfilesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<List<PostingProfileDto>>> GetAll(CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new GetPostingProfilesQuery(), cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PostingProfileDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new GetPostingProfileByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<PostingProfileDto>> Create([FromBody] CreatePostingProfileDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new CreatePostingProfileCommand(dto, CurrentUserId), cancellationToken));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PostingProfileDto>> Update(Guid id, [FromBody] UpdatePostingProfileDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new UpdatePostingProfileCommand(id, dto), cancellationToken));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<Guid>> Delete(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new DeletePostingProfileCommand(id), cancellationToken));
    }
}
