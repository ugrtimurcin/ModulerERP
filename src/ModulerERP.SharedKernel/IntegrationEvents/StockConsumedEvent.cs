using ModulerERP.SharedKernel.Interfaces;

namespace ModulerERP.SharedKernel.IntegrationEvents;

public class StockConsumedEvent : IDomainEvent
{
    public Guid TenantId { get; }
    public Guid MovementId { get; }
    public Guid? ProjectId { get; }
    public decimal Quantity { get; }
    public decimal CostPrice { get; }
    public Guid CurrencyId { get; }
    public DateTime Date { get; }

    public decimal TotalCost { get; } // Calculated Cost
    public string ProductName { get; }
    public string UnitOfMeasure { get; }

    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    public StockConsumedEvent(Guid tenantId, Guid movementId, Guid? projectId, decimal quantity, decimal costPrice, Guid currencyId, DateTime date, decimal totalCost, string productName, string unitOfMeasure)
    {
        TenantId = tenantId;
        MovementId = movementId;
        ProjectId = projectId;
        Quantity = quantity;
        CostPrice = costPrice;
        CurrencyId = currencyId;
        Date = date;
        TotalCost = totalCost;
        ProductName = productName;
        UnitOfMeasure = unitOfMeasure;
    }
}
