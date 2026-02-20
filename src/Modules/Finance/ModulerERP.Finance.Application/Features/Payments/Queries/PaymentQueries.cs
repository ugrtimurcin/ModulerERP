using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.Payments.Queries;

// ── Get All Payments ──
public record GetPaymentsQuery : IRequest<Result<List<PaymentDto>>>;

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, Result<List<PaymentDto>>>
{
    private readonly IRepository<Payment> _repository;
    private readonly IRepository<Account> _accountRepository;
    private readonly ICurrencyLookupService _currencyLookupService;

    public GetPaymentsQueryHandler(
        IRepository<Payment> repository,
        IRepository<Account> accountRepository,
        ICurrencyLookupService currencyLookupService)
    {
        _repository = repository;
        _accountRepository = accountRepository;
        _currencyLookupService = currencyLookupService;
    }

    public async Task<Result<List<PaymentDto>>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await _repository.GetAllAsync(cancellationToken);
        var accounts = await _accountRepository.GetAllAsync(cancellationToken);
        var currenciesResult = await _currencyLookupService.GetActiveCurrenciesAsync(cancellationToken);
        var currencies = currenciesResult.IsSuccess ? currenciesResult.Value! : new List<ModulerERP.SharedKernel.DTOs.CurrencyLookupDto>();
        
        var dtos = payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            PaymentNumber = p.PaymentNumber,
            Direction = p.Direction.ToString(),
            Method = p.Method.ToString(),
            PartnerId = p.PartnerId,
            PartnerName = "", // TODO: Inject Partner Service or use event sourcing
            Amount = p.Amount,
            CurrencyCode = currencies.FirstOrDefault(c => c.Id == p.CurrencyId)?.Code ?? "TRY",
            AccountId = p.AccountId,
            AccountName = accounts.FirstOrDefault(a => a.Id == p.AccountId)?.Name ?? "Unknown",
            PaymentDate = p.PaymentDate,
            ReferenceNumber = p.ReferenceNumber,
            Status = "Posted"
        }).OrderByDescending(p => p.PaymentDate).ToList();

        return Result<List<PaymentDto>>.Success(dtos);
    }
}

// ── Get Payment By Id ──
public record GetPaymentByIdQuery(Guid Id) : IRequest<Result<PaymentDto>>;

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
{
    private readonly IRepository<Payment> _repository;
    private readonly IRepository<Account> _accountRepository;
    private readonly ICurrencyLookupService _currencyLookupService;

    public GetPaymentByIdQueryHandler(
        IRepository<Payment> repository,
        IRepository<Account> accountRepository,
        ICurrencyLookupService currencyLookupService)
    {
        _repository = repository;
        _accountRepository = accountRepository;
        _currencyLookupService = currencyLookupService;
    }

    public async Task<Result<PaymentDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (payment == null) return Result<PaymentDto>.Failure("Payment not found");

        var account = await _accountRepository.GetByIdAsync(payment.AccountId, cancellationToken);
        var currenciesResult = await _currencyLookupService.GetActiveCurrenciesAsync(cancellationToken);
        var currencyStr = "TRY";
        if (currenciesResult.IsSuccess)
        {
            var curr = currenciesResult.Value!.FirstOrDefault(c => c.Id == payment.CurrencyId);
            if (curr != null) currencyStr = curr.Code;
        }

        var dto = new PaymentDto
        {
            Id = payment.Id,
            PaymentNumber = payment.PaymentNumber,
            Direction = payment.Direction.ToString(),
            Method = payment.Method.ToString(),
            PartnerId = payment.PartnerId,
            PartnerName = "", // External module lookup required
            Amount = payment.Amount,
            CurrencyCode = currencyStr,
            AccountId = payment.AccountId,
            AccountName = account?.Name ?? "Unknown",
            PaymentDate = payment.PaymentDate,
            ReferenceNumber = payment.ReferenceNumber,
            Status = "Posted"
        };
        return Result<PaymentDto>.Success(dto);
    }
}
