namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Available system languages for TRNC bilingual support (Turkish/English).
/// </summary>
public class Language
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    
    /// <summary>Language code (e.g., 'tr-TR', 'en-US')</summary>
    public string Code { get; private set; } = string.Empty;
    
    /// <summary>Language name</summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>Right-to-Left support (e.g., Arabic)</summary>
    public bool IsRtl { get; private set; }
    
    public bool IsActive { get; private set; } = true;

    private Language() { } // EF Core

    public static Language Create(string code, string name, bool isRtl = false)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Language code is required", nameof(code));

        return new Language
        {
            Code = code,
            Name = name,
            IsRtl = isRtl
        };
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
