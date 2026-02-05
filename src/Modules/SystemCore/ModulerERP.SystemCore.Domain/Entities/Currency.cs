using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Currency entity - Global ISO 4217 currency definitions.
/// NOT tenant-specific as currencies are universal facts.
/// </summary>
public class Currency
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    /// <summary>ISO 4217 Code (e.g., 'TRY', 'USD', 'GBP', 'EUR')</summary>
    public string Code { get; private set; } = string.Empty;
    
    /// <summary>Full name (e.g., 'Turkish Lira')</summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>Symbol (e.g., '₺', '$', '£', '€')</summary>
    public string Symbol { get; private set; } = string.Empty;
    
    /// <summary>Decimal precision (e.g., 2 for most currencies)</summary>
    public int Precision { get; private set; } = 2;
    
    /// <summary>System-wide visibility</summary>
    public bool IsActive { get; private set; } = true;

    private Currency() { } // EF Core

    public static Currency Create(string code, string name, string symbol, int precision = 2)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
            throw new ArgumentException("Currency code must be 3 characters (ISO 4217)", nameof(code));

        return new Currency
        {
            Code = code.ToUpperInvariant(),
            Name = name,
            Symbol = symbol,
            Precision = precision
        };
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
