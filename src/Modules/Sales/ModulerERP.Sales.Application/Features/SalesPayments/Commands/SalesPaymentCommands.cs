using MediatR;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Sales.Application.Features.SalesPayments.Commands;

// ── DTOs ──
public record SalesPaymentDto(
    Guid Id, Guid InvoiceId, Guid? PaymentId, decimal AllocatedAmount,
    DateTime AllocationDate, string? PaymentMethod, string? ReferenceNumber, string? Notes,
    DateTime CreatedAt);

// ── Create ──
public record CreateSalesPaymentCommand(
    Guid InvoiceId, decimal AllocatedAmount, DateTime AllocationDate,
    Guid? PaymentId = null, string? PaymentMethod = null,
    string? ReferenceNumber = null, string? Notes = null) : IRequest<SalesPaymentDto>;

public class CreateSalesPaymentCommandHandler : IRequestHandler<CreateSalesPaymentCommand, SalesPaymentDto>
{
    private readonly IRepository<SalesPayment> _paymentRepo;
    private readonly IRepository<Invoice> _invoiceRepo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateSalesPaymentCommandHandler(
        IRepository<SalesPayment> paymentRepo, IRepository<Invoice> invoiceRepo,
        ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _paymentRepo = paymentRepo; _invoiceRepo = invoiceRepo; _uow = uow; _currentUser = currentUser; }

    public async Task<SalesPaymentDto> Handle(CreateSalesPaymentCommand r, CancellationToken ct)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(r.InvoiceId, ct)
            ?? throw new KeyNotFoundException($"Invoice '{r.InvoiceId}' not found.");

        var payment = SalesPayment.Create(
            _currentUser.TenantId, r.InvoiceId, r.AllocatedAmount, r.AllocationDate,
            _currentUser.UserId, r.PaymentId, r.PaymentMethod, r.ReferenceNumber, r.Notes);

        // Also update invoice paid amount
        invoice.RecordPayment(r.AllocatedAmount);

        await _paymentRepo.AddAsync(payment, ct);
        await _uow.SaveChangesAsync(ct);

        return SalesPaymentMapper.ToDto(payment);
    }
}

// ── Delete ──
public record DeleteSalesPaymentCommand(Guid Id) : IRequest;

public class DeleteSalesPaymentCommandHandler : IRequestHandler<DeleteSalesPaymentCommand>
{
    private readonly IRepository<SalesPayment> _repo;
    private readonly ISalesUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public DeleteSalesPaymentCommandHandler(IRepository<SalesPayment> repo, ISalesUnitOfWork uow, ICurrentUserService currentUser)
    { _repo = repo; _uow = uow; _currentUser = currentUser; }

    public async Task Handle(DeleteSalesPaymentCommand request, CancellationToken ct)
    {
        var payment = await _repo.GetByIdAsync(request.Id, ct)
            ?? throw new KeyNotFoundException($"SalesPayment '{request.Id}' not found.");
        payment.Delete(_currentUser.UserId);
        await _uow.SaveChangesAsync(ct);
    }
}

// ── Mapper ──
internal static class SalesPaymentMapper
{
    internal static SalesPaymentDto ToDto(SalesPayment sp) => new(
        sp.Id, sp.InvoiceId, sp.PaymentId, sp.AllocatedAmount,
        sp.AllocationDate, sp.PaymentMethod, sp.ReferenceNumber, sp.Notes,
        sp.CreatedAt);
}
