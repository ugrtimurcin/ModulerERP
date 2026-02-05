using ModulerERP.SharedKernel.Entities;
using ModulerERP.SystemCore.Domain.Enums;

namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Email/SMS template with language support.
/// </summary>
public class NotificationTemplate : BaseEntity
{
    /// <summary>Template code (e.g., 'INVOICE_PAID', 'ORDER_SHIPPED')</summary>
    public string Code { get; private set; } = string.Empty;
    
    public NotificationChannel Channel { get; private set; }
    
    /// <summary>Email subject (null for SMS)</summary>
    public string? Subject { get; private set; }
    
    /// <summary>HTML content with placeholders</summary>
    public string BodyHtml { get; private set; } = string.Empty;
    
    /// <summary>Language code (e.g., 'tr-TR')</summary>
    public string LanguageCode { get; private set; } = string.Empty;

    private NotificationTemplate() { } // EF Core

    public static NotificationTemplate Create(
        Guid tenantId, 
        string code, 
        NotificationChannel channel, 
        string bodyHtml, 
        string languageCode,
        Guid createdByUserId,
        string? subject = null)
    {
        var template = new NotificationTemplate
        {
            Code = code.ToUpperInvariant(),
            Channel = channel,
            Subject = subject,
            BodyHtml = bodyHtml,
            LanguageCode = languageCode
        };

        template.SetTenant(tenantId);
        template.SetCreator(createdByUserId);
        return template;
    }

    public void UpdateContent(string? subject, string bodyHtml)
    {
        Subject = subject;
        BodyHtml = bodyHtml;
    }
}
