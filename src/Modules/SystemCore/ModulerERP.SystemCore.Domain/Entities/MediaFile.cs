using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Centralized DMS for all file types.
/// Uses polymorphic associations for flexibility.
/// </summary>
public class MediaFile : BaseEntity
{
    /// <summary>Original filename</summary>
    public string FileName { get; private set; } = string.Empty;
    
    /// <summary>Storage path (S3/Disk)</summary>
    public string StoragePath { get; private set; } = string.Empty;
    
    /// <summary>File extension (.pdf, .jpg)</summary>
    public string Extension { get; private set; } = string.Empty;
    
    /// <summary>MIME type (application/pdf)</summary>
    public string MimeType { get; private set; } = string.Empty;
    
    /// <summary>File size in bytes</summary>
    public long SizeInBytes { get; private set; }
    
    /// <summary>Polymorphic: 'UserAvatar', 'InvoiceDoc', 'ProductImage'</summary>
    public string EntityType { get; private set; } = string.Empty;
    
    /// <summary>Polymorphic: ID of the related record</summary>
    public Guid EntityId { get; private set; }

    private MediaFile() { } // EF Core

    public static MediaFile Create(
        Guid tenantId, 
        string fileName, 
        string storagePath, 
        string mimeType, 
        long sizeInBytes,
        string entityType, 
        Guid entityId,
        Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required", nameof(fileName));

        var extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;

        var mediaFile = new MediaFile
        {
            FileName = fileName,
            StoragePath = storagePath,
            Extension = extension,
            MimeType = mimeType,
            SizeInBytes = sizeInBytes,
            EntityType = entityType,
            EntityId = entityId
        };

        mediaFile.SetTenant(tenantId);
        mediaFile.SetCreator(createdByUserId);
        return mediaFile;
    }

    public void UpdateStoragePath(string newPath) => StoragePath = newPath;
}
