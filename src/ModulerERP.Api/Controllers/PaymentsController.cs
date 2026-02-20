using Microsoft.AspNetCore.Mvc;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using MediatR;

namespace ModulerERP.Api.Controllers;

public class PaymentsController : BaseApiController
{
    private readonly ISender _sender;

    public PaymentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<List<PaymentDto>>> GetAll(CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.Payments.Queries.GetPaymentsQuery(), cancellationToken));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PaymentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.Payments.Queries.GetPaymentByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create(CreatePaymentDto dto, CancellationToken cancellationToken)
    {
        return HandleResult(await _sender.Send(new ModulerERP.Finance.Application.Features.Payments.Commands.CreatePaymentCommand(dto, CurrentUserId), cancellationToken));
    }
}
