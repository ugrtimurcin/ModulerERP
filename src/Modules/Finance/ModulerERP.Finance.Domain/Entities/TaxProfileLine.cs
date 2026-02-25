namespace ModulerERP.Finance.Domain.Entities;

/// <summary>
/// A single rule within a Tax Profile mapping to a TaxRate.
/// </summary>
public class TaxProfileLine
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    public Guid TaxProfileId { get; private set; }
    
    public Guid TaxRateId { get; private set; }
    
    public bool IsInclusive { get; private set; }
    
    public int CalculationOrder { get; private set; }

    // Navigation
    public TaxProfile? TaxProfile { get; private set; }
    public TaxRate? TaxRate { get; private set; }

    private TaxProfileLine() { }

    internal static TaxProfileLine Create(Guid taxProfileId, Guid taxRateId, bool isInclusive, int calculationOrder)
    {
        return new TaxProfileLine
        {
            TaxProfileId = taxProfileId,
            TaxRateId = taxRateId,
            IsInclusive = isInclusive,
            CalculationOrder = calculationOrder
        };
    }
}
