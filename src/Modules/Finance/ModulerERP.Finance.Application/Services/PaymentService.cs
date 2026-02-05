using ModulerERP.Finance.Application.DTOs;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.Finance.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;
using ModulerERP.SharedKernel.Results;
using ModulerERP.Finance.Domain.Enums;


namespace ModulerERP.Finance.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IRepository<Payment> _repository;
    private readonly IRepository<Account> _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IRepository<Payment> repository, 
        IRepository<Account> accountRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<PaymentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var payments = await _repository.GetAllAsync(cancellationToken);
        
        // Optimize: Fetch Account Names
        var accountIds = payments.Select(p => p.AccountId).Distinct().ToList();
        // Since Generic Repo might not have spec, we fetch all or optimize later. 
        // For MVP, assuming small number.
        // Actually, Payment has Reference to Account. EF Core might load if we Included. 
        // The Generic Repo usually returns List. 
        // We'll map what we have.
        
        var dtos = payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            PaymentNumber = p.PaymentNumber,
            Direction = p.Direction.ToString(),
            Method = p.Method.ToString(),
            PartnerId = p.PartnerId,
            PartnerName = "Partner Name Loader Needed", // TODO: Inject Partner Service or use event sourcing
            Amount = p.Amount,
            CurrencyCode = "TRY", // Todo: Load Currency
            AccountId = p.AccountId,
            AccountName = "Bank/Cash Account", // Todo: Load Account
            PaymentDate = p.PaymentDate,
            ReferenceNumber = p.ReferenceNumber,
            Status = "Posted"
        }).OrderByDescending(p => p.PaymentDate).ToList();

        return Result<List<PaymentDto>>.Success(dtos);
    }

    public async Task<Result<PaymentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _repository.GetByIdAsync(id, cancellationToken);
        if (payment == null) return Result<PaymentDto>.Failure("Payment not found");

        var dto = new PaymentDto
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
        return Result<PaymentDto>.Success(dto);
    }

    public async Task<Result<PaymentDto>> CreateAsync(CreatePaymentDto dto, Guid createdByUserId, CancellationToken cancellationToken = default)
    {
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
            Guid.Empty, // Tenant
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(payment.Id, cancellationToken);
    }
}
