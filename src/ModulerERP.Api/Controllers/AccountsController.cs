using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;

namespace ModulerERP.Api.Controllers;

public class AccountsController : BaseApiController
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAll(CancellationToken cancellationToken)
    {
        return HandleResult(await _accountService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AccountDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _accountService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<AccountDto>> Create(CreateAccountDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _accountService.CreateAsync(dto, CurrentUserId, cancellationToken));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AccountDto>> Update(Guid id, UpdateAccountDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _accountService.UpdateAsync(id, dto, cancellationToken));
    }
}
