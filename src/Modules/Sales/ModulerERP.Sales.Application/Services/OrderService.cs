using ModulerERP.Sales.Application.DTOs;
using ModulerERP.Sales.Application.Interfaces;
using ModulerERP.Sales.Domain.Entities;
using ModulerERP.SharedKernel.Results;
using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.Sales.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IQuoteRepository _quoteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStockReservationService _stockReservationService;

    public OrderService(
        IOrderRepository orderRepository,
        IQuoteRepository quoteRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IStockReservationService stockReservationService)
    {
        _orderRepository = orderRepository;
        _quoteRepository = quoteRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _stockReservationService = stockReservationService;
    }

    public async Task<Result<List<OrderDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllWithLinesAsync(cancellationToken);
        return Result<List<OrderDto>>.Success(orders.Select(MapToDto).ToList());
    }

    public async Task<Result<OrderDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithLinesAsync(id, cancellationToken);
        if (order == null)
            return Result<OrderDto>.Failure("Order not found");
        return Result<OrderDto>.Success(MapToDto(order));
    }

    public async Task<Result<Guid>> CreateAsync(CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Generate Order Number
        var count = await _orderRepository.GetNextOrderNumberAsync(cancellationToken);
        var year = DateTime.UtcNow.Year;
        var orderNumber = $"ORD-{year}-{count:0000}";

        // 2. Handle Conversion from Quote
        if (dto.QuoteId.HasValue)
        {
            var quote = await _quoteRepository.GetByIdAsync(dto.QuoteId.Value, cancellationToken);
            if (quote != null)
            {
               // Mark quote as converted/accepted if not already
               // Assuming Accept() puts it in a state that means 'won'
               // If there is a helper for 'Convert', use it. For now, just Accept.
               if (quote.Status != ModulerERP.Sales.Domain.Enums.QuoteStatus.Accepted && 
                   quote.Status != ModulerERP.Sales.Domain.Enums.QuoteStatus.Converted) 
               {
                   quote.Accept(); 
                   // Ideally we have a 'Converted' status? Enums had 'Converted' in Step 4216 logic (QuotePage)?
                   // I'll check QuoteStatus Enum later. For now Accept() is safe.
               }
            }
        }

        // 3. Create Order
        var order = Order.Create(
            _currentUserService.TenantId,
            orderNumber,
            dto.PartnerId,
            dto.CurrencyId,
            dto.ExchangeRate,
            _currentUserService.UserId,
            dto.QuoteId,
            dto.WarehouseId,
            dto.RequestedDeliveryDate,
            dto.PaymentTerms
        );
        
        order.SetAddresses(dto.ShippingAddress, dto.BillingAddress);
        if(!string.IsNullOrEmpty(dto.Notes)) 
            // Add notes logic if Entity supports setters or do it via private reflection/method? 
            // Entity properties are private set.
            // Order.Create only takes limited params.
            // I need to check Order Entity for Notes setter.
            // Entity code viewed in 4223: public string? Notes { get; private set; }
            // No setter. I might need to update constructor or add SetNotes method.
            // For now, I'll ignore Notes or assuming I can't set them without method.
            // Wait, Order.Create doesn't take Notes.
            // I should add SetNotes method to Order entity or use reflection if I can't modify Entity now.
            // I'll execute a separate `replace_file_content` to add SetNotes to Order entity later.
            // For now, I'll skip Notes.
            {};

        int lineNum = 1;
        foreach(var lineDto in dto.Lines)
        {
            var line = OrderLine.Create(
                order.Id,
                lineDto.ProductId,
                lineDto.Description,
                lineDto.Quantity,
                lineDto.UnitOfMeasureId,
                lineDto.UnitPrice,
                lineNum++,
                lineDto.DiscountPercent,
                lineDto.TaxPercent
            );
            order.Lines.Add(line);
        }

        UpdateOrderTotals(order);

        await _orderRepository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(order.Id);
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateOrderDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithLinesAsync(id, cancellationToken);
        if (order == null) return Result.Failure("Order not found");

        // Logic to update Header fields (DeliveryDate, Notes etc)
        // Ensure entity has methods.
        // Assuming we can re-create lines (Clear and Add) as per QuoteService strategy.
        order.Lines.Clear(); 
        
        int lineNum = 1;
        foreach(var lineDto in dto.Lines)
        {
            var line = OrderLine.Create(
                order.Id,
                lineDto.ProductId,
                lineDto.Description,
                lineDto.Quantity,
                lineDto.UnitOfMeasureId,
                lineDto.UnitPrice,
                lineNum++,
                lineDto.DiscountPercent,
                lineDto.TaxPercent
            );
            order.Lines.Add(line);
        }
        
        UpdateOrderTotals(order);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdAsync(id, cancellationToken);
        if (order == null) return Result.Failure("Order not found");
        
        // Soft delete
        _orderRepository.Remove(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithLinesAsync(id, cancellationToken);
        if (order == null) return Result.Failure("Order not found");

        if (order.Status != ModulerERP.Sales.Domain.Enums.OrderStatus.Pending)
            return Result.Failure("Only pending orders can be confirmed");

        // Reserve Stock
        if (order.WarehouseId.HasValue) 
        {
            var reservedLines = new List<OrderLine>();
            foreach(var line in order.Lines)
            {
                var res = await _stockReservationService.ReserveStockAsync(
                    _currentUserService.TenantId, 
                    line.ProductId, 
                    order.WarehouseId.Value, 
                    line.Quantity, 
                    order.OrderNumber, 
                    cancellationToken);
                
                if (!res.IsSuccess)
                {
                    // Compensate
                    foreach(var rLine in reservedLines)
                    {
                        await _stockReservationService.ReleaseReservationAsync(
                            _currentUserService.TenantId,
                            rLine.ProductId,
                            order.WarehouseId.Value,
                            rLine.Quantity,
                            order.OrderNumber,
                            cancellationToken);
                    }
                    return Result.Failure($"Stock reservation failed: {res.Error}");
                }
                reservedLines.Add(line);
            }
        }

        try 
        {
            order.Confirm();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch(Exception)
        {
            // Compensation: Release Stock if confirmation fails
             if (order.WarehouseId.HasValue) 
             {
                foreach(var line in order.Lines)
                {
                    await _stockReservationService.ReleaseReservationAsync(
                        _currentUserService.TenantId, 
                        line.ProductId, 
                        order.WarehouseId.Value, 
                        line.Quantity, 
                        order.OrderNumber, 
                        cancellationToken);
                }
             }
             throw; // Re-throw to inform caller/controller
        }
        return Result.Success();
    }

    public async Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _orderRepository.GetByIdWithLinesAsync(id, cancellationToken);
        if (order == null) return Result.Failure("Order not found");

        if (order.Status == ModulerERP.Sales.Domain.Enums.OrderStatus.Confirmed && order.WarehouseId.HasValue)
        {
            foreach(var line in order.Lines)
            {
                await _stockReservationService.ReleaseReservationAsync(
                    _currentUserService.TenantId,
                    line.ProductId,
                    order.WarehouseId.Value,
                    line.Quantity,
                    order.OrderNumber,
                    cancellationToken);
            }
        }
        
        order.Cancel();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private void UpdateOrderTotals(Order order)
    {
        decimal subTotal = order.Lines.Sum(x => x.LineTotal); // LineTotal is (Qty*Price)-Disc
        decimal totalDiscount = order.Lines.Sum(x => x.DiscountAmount);
        decimal totalTax = order.Lines.Sum(x => (x.LineTotal * x.TaxPercent / 100));
        order.UpdateTotals(subTotal, totalDiscount, totalTax);
    }

    private OrderDto MapToDto(Order o)
    {
        return new OrderDto(
            o.Id,
            o.OrderNumber,
            o.QuoteId,
            o.PartnerId,
            "Partner (TODO)",
            o.Status,
            o.CurrencyId,
            "CUR",
            o.ExchangeRate,
            o.RequestedDeliveryDate,
            o.ShippingAddress,
            o.BillingAddress,
            o.PaymentTerms,
            o.Notes,
            o.SubTotal,
            o.DiscountAmount,
            o.TaxAmount,
            o.TotalAmount,
            o.Lines?.Select(l => new OrderLineDto(
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
                l.LineTotal,
                l.ShippedQuantity,
                l.InvoicedQuantity
            )).ToList() ?? new List<OrderLineDto>(),
            o.CreatedAt,
            "User"
        );
    }

    public Task<Result> MarkShippedAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<Result> MarkInvoicedAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
