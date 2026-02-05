namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Dynamic UI strings for localization.
/// </summary>
public class Translation
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    /// <summary>Language code (e.g., 'tr-TR')</summary>
    public string LanguageCode { get; private set; } = string.Empty;
    
    /// <summary>Translation key (e.g., 'COMMON.SAVE', 'INVOICE.TITLE')</summary>
    public string Key { get; private set; } = string.Empty;
    
    /// <summary>Translated value</summary>
    public string Value { get; private set; } = string.Empty;

    private Translation() { } // EF Core

    public static Translation Create(string languageCode, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Translation key is required", nameof(key));

        return new Translation
        {
            LanguageCode = languageCode,
            Key = key.ToUpperInvariant(),
            Value = value
        };
    }

    public void UpdateValue(string value) => Value = value;
}
