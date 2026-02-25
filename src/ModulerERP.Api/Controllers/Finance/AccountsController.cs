using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using MediatR;
using ModulerERP.Finance.Application.Interfaces;

namespace ModulerERP.Api.Controllers.Finance;

[Route("api/finance/accounts")]
public class AccountsController : BaseApiController
{
    private readonly ISender _sender;

    public AccountsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAll(CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.Accounts.Queries.GetAccountsQuery(), cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.Accounts.Queries.GetAccountByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create(CreateAccountDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.Accounts.Commands.CreateAccountCommand(dto, CurrentUserId), cancellationToken));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AccountDto>> Update(Guid id, UpdateAccountDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.Accounts.Commands.UpdateAccountCommand(id, dto), cancellationToken));
    }
}
