using ModulerERP.Sales.Application.DTOs;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;
using ModulerERP.SharedKernel.Results;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Sales.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFinanceOperationsService _financeOperationsService;

    public InvoiceService(
        IInvoiceRepository invoiceRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IFinanceOperationsService financeOperationsService)
    {
        _invoiceRepository = invoiceRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _financeOperationsService = financeOperationsService;
    }

    public async Task<Result<List<InvoiceDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var invoices = await _invoiceRepository.GetAllWithLinesAsync(cancellationToken);
        return Result<List<InvoiceDto>>.Success(invoices.Select(MapToDto).ToList());
    }

    public async Task<Result<InvoiceDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdWithLinesAsync(id, cancellationToken);
        if (invoice == null) return Result<InvoiceDto>.Failure("Invoice not found");
        return Result<InvoiceDto>.Success(MapToDto(invoice));
    }

    public async Task<Result<Guid>> CreateAsync(CreateInvoiceDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Generate Invoice Number
        var count = await _invoiceRepository.GetNextInvoiceNumberAsync(cancellationToken);
        var year = DateTime.UtcNow.Year;
        var invoiceNumber = $"INV-{year}-{count:0000}";

        // 2. Validate Order if present
        if (dto.OrderId.HasValue)
        {
            var order = await _orderRepository.GetByIdAsync(dto.OrderId.Value, cancellationToken);
            if (order == null) return Result<Guid>.Failure("Order not found");
            // Could validate order status here (e.g. must be Confirmed or Shipped)
            if (order.Status == ModulerERP.Sales.Domain.Enums.OrderStatus.Pending || 
                order.Status == ModulerERP.Sales.Domain.Enums.OrderStatus.Cancelled)
            {
                return Result<Guid>.Failure($"Cannot invoice order in status {order.Status}");
            }
        }

        // 3. Create Invoice
        var invoice = Invoice.Create(
            _currentUserService.TenantId,
            invoiceNumber,
            dto.PartnerId,
            dto.CurrencyId,
            dto.ExchangeRate,
            dto.InvoiceDate,
            dto.DueDate,
            _currentUserService.UserId,
            dto.OrderId,
            dto.PaymentTerms
        );

        invoice.SetAddresses(dto.ShippingAddress, dto.BillingAddress);
        // Note: Invoice entity might need SetNotes method similar to Order
        
        int lineNum = 1;
        foreach(var lineDto in dto.Lines)
        {
            var line = InvoiceLine.Create(
                invoice.Id,
                lineDto.ProductId,
                lineDto.Description,
                lineDto.Quantity,
                lineDto.UnitOfMeasureId,
                lineDto.UnitPrice,
                lineNum++,
                lineDto.DiscountPercent,
                lineDto.TaxPercent,
                lineDto.OrderLineId
            );
            invoice.Lines.Add(line);
        }

        UpdateInvoiceTotals(invoice);

        await _invoiceRepository.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: Update Order Invoiced Quantity if linked?
        // This should technically happen when Invoice is Issued/Posted, not just Draft.
        
        return Result<Guid>.Success(invoice.Id);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateInvoiceDto dto, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdWithLinesAsync(id, cancellationToken);
        if (invoice == null) return Result.Failure("Invoice not found");

        if (invoice.Status != ModulerERP.Sales.Domain.Enums.InvoiceStatus.Draft)
            return Result.Failure("Only draft invoices can be updated");

        // Logic to update header fields (needs specific methods on Entity)
        // For now, assuming entity methods or direct property access if internal.
        // Actually Invoice entity properties are private set.
        // Needs SetDetails method. 
        // Assuming we re-create lines strategy similar to Orders/Quotes.
        
        invoice.Lines.Clear();
        int lineNum = 1;
        foreach(var lineDto in dto.Lines)
        {
            var line = InvoiceLine.Create(
                invoice.Id,
                lineDto.ProductId,
                lineDto.Description,
                lineDto.Quantity,
                lineDto.UnitOfMeasureId,
                lineDto.UnitPrice,
                lineNum++,
                lineDto.DiscountPercent,
                lineDto.TaxPercent,
                lineDto.OrderLineId
            );
            invoice.Lines.Add(line);
        }
        
        UpdateInvoiceTotals(invoice);
        
        // Update header fields via separate call if needed, or assume entity has setters we can access via reflection/method
        invoice.SetAddresses(dto.ShippingAddress, dto.BillingAddress);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null) return Result.Failure("Invoice not found");
        
        if (invoice.Status != ModulerERP.Sales.Domain.Enums.InvoiceStatus.Draft)
             return Result.Failure("Only draft invoices can be deleted");

        _invoiceRepository.Remove(invoice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> IssueAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdWithLinesAsync(id, cancellationToken);
        if (invoice == null) return Result.Failure("Invoice not found");
        
        if (invoice.Status != ModulerERP.Sales.Domain.Enums.InvoiceStatus.Draft)
             return Result.Failure("Invoice is not in Draft status");

        invoice.Issue();
        
        // Finance Integration (Create Receivable)
        var financeResult = await _financeOperationsService.CreateReceivableAsync(
            _currentUserService.TenantId,
            invoice.InvoiceNumber,
            invoice.PartnerId,
            invoice.TotalAmount,
            "USD", // TODO: Use invoice.CurrencyCode when available in Entity
            invoice.InvoiceDate,
            invoice.DueDate,
            invoice.Id,
            $"Invoice {invoice.InvoiceNumber}",
            cancellationToken
        );

        if (!financeResult.IsSuccess)
            return Result.Failure($"Finance Error: {financeResult.Error}");

        // TODO: Update Order Invoiced Quantities
        if (invoice.OrderId.HasValue)
        {
             // Logic to update Order.InvoicedQuantity would go here.
             // Requires OrderService or Domain Event? 
             // Ideally Domain Event: InvoiceIssued -> OrderHandler updates Order.
             // For now, keep it simple?
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id, cancellationToken);
        if (invoice == null) return Result.Failure("Invoice not found");
        
        invoice.Cancel();
        // TODO: Reverse Finance Entries
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private void UpdateInvoiceTotals(Invoice invoice)
    {
        decimal subTotal = invoice.Lines.Sum(x => x.LineTotal); // InvoiceLine.LineTotal includes Tax? No, let's check InvoiceLine.
        // InvoiceLine: LineTotal = (Qty*Price - Disc) + Tax. 
        // Wait, InvoiceLine.CalculateTotals:
        // gross = Qty * Price
        // Disc = gross * %
        // net = gross - Disc
        // Tax = net * %
        // LineTotal = net + Tax.
        // So LineTotal is inc tax.
        
        // Invoice Header wants: SubTotal, DiscountAmount, TaxAmount, TotalAmount.
        // SubTotal usually excludes tax?
        // Let's sum components.
        
        decimal totalDiscount = invoice.Lines.Sum(x => x.DiscountAmount);
        decimal totalTax = invoice.Lines.Sum(x => x.TaxAmount);
        
        // Gross subtotal (before discount)
        decimal grossSubTotal = invoice.Lines.Sum(x => x.Quantity * x.UnitPrice);
        // OR Net subtotal (after discount, before tax). 
        // Typically SubTotal in UI is "Sum of Line Net Amounts" or "Sum of Line Gross Amounts".
        // Let's trust the logic: TotalAmount = SubTotal - Discount + Tax.
        // If LineTotal = (Gross - Disc) + Tax
        // Sum(LineTotal) = Sum(Gross) - Sum(Disc) + Sum(Tax).
        // So SubTotal should be Sum(Gross).
        
        invoice.UpdateTotals(grossSubTotal, totalDiscount, totalTax);
    }

    private InvoiceDto MapToDto(Invoice i)
    {
        return new InvoiceDto(
            i.Id,
            i.InvoiceNumber,
            i.OrderId,
            i.Order?.OrderNumber ?? "",
            i.PartnerId,
            "Partner (TODO)",
            i.Status,
            i.CurrencyId,
            "CUR",
            i.ExchangeRate,
            i.InvoiceDate,
            i.DueDate,
            i.ShippingAddress,
            i.BillingAddress,
            i.PaymentTerms,
            i.Notes,
            i.SubTotal,
            i.DiscountAmount,
            i.TaxAmount,
            i.TotalAmount,
            i.PaidAmount,
            i.BalanceDue,
            i.Lines?.Select(l => new InvoiceLineDto(
                l.Id,
                l.ProductId,
                "Product (TODO)",
                l.Description,
                l.Quantity,
                l.UnitOfMeasureId,
                "UOM",
                l.UnitPrice,
                l.DiscountPercent,
                l.DiscountAmount,
                l.TaxPercent,
                l.LineTotal
            )).ToList() ?? new List<InvoiceLineDto>(),
            i.CreatedAt,
            "User"
        );
    }
}
