using MediatR;
using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.Finance.Domain.Enums;
using ModulerERP.Finance.Domain.Events;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModulerERP.Finance.Application.Features.Payments.Commands;

public record CreatePaymentCommand(CreatePaymentDto Dto, Guid CreatedByUserId) : IRequest<Result<PaymentDto>>;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<PaymentDto>>
{
    private readonly IRepository<Payment> _repository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublisher _publisher;

    public CreatePaymentCommandHandler(
        IRepository<Payment> repository, 
        IRepository<Account> accountRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPublisher publisher)
    {
        _repository = repository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _publisher = publisher;
    }

    public async Task<Result<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var createdByUserId = request.CreatedByUserId;

        // 1. Validate Account
        var account = await _accountRepository.GetByIdAsync(dto.AccountId, cancellationToken);
        if (account == null)
            return Result<PaymentDto>.Failure("Selected Bank/Cash Account not found.");

        if (!account.IsBankAccount && account.Type != AccountType.Asset) 
             // Ideally check IsBankAccount flag, but relying on Type for MVP fallback
             return Result<PaymentDto>.Failure("Selected account must be a Bank or Cash account.");

        // 2. Generate Number (Simple logic for MVP)
        var count = (await _repository.GetAllAsync(cancellationToken)).Count();
        string number = $"PAY-{DateTime.Now.Year}-{(count + 1):D4}";

        // 3. Create Entity
        var payment = Payment.Create(
            _currentUserService.TenantId, // Tenant
            number,
            dto.Direction,
            dto.Method,
            dto.PartnerId,
            dto.CurrencyId,
            1, // Exchange Rate
            dto.Amount,
            dto.AccountId,
            dto.PaymentDate,
            createdByUserId,
            dto.ReferenceNumber
        );

        await _repository.AddAsync(payment, cancellationToken);
        
        // Dispatch Domain Event to trigger Journal Entry creation in the same transaction space
        await _publisher.Publish(new PaymentCreatedEvent(
            payment.TenantId,
            payment.Id,
            payment.Amount,
            payment.AccountId,
            payment.PaymentNumber
        ), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map to DTO natively
        var resultDto = new PaymentDto
        {
            Id = payment.Id,
            PaymentNumber = payment.PaymentNumber,
            Direction = payment.Direction.ToString(),
            Method = payment.Method.ToString(),
            PartnerId = payment.PartnerId,
            Amount = payment.Amount,
            AccountId = payment.AccountId,
            PaymentDate = payment.PaymentDate,
            ReferenceNumber = payment.ReferenceNumber
        };

        return Result<PaymentDto>.Success(resultDto);
    }
}
