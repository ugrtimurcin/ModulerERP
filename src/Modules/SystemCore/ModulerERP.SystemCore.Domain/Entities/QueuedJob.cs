using ModulerERP.SystemCore.Domain.Enums;

namespace ModulerERP.SystemCore.Domain.Entities;

/// <summary>
/// Async job queue for heavy tasks (Emails, Reports).
/// Prevents UI freezing by offloading work.
/// </summary>
public class QueuedJob
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; private set; }
    
    /// <summary>Job type (e.g., 'ReportGenerator', 'EmailSender')</summary>
    public string JobType { get; private set; } = string.Empty;
    
    /// <summary>Job parameters as JSON</summary>
    public string Payload { get; private set; } = "{}";
    
    public JobStatus Status { get; private set; } = JobStatus.Pending;
    
    public int TryCount { get; private set; }
    public DateTime? NextTryAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; private set; }

    private QueuedJob() { } // EF Core

    public static QueuedJob Create(Guid tenantId, string jobType, string payload)
    {
        return new QueuedJob
        {
            TenantId = tenantId,
            JobType = jobType,
            Payload = payload,
            NextTryAt = DateTime.UtcNow
        };
    }

    public void MarkProcessing()
    {
        Status = JobStatus.Processing;
        TryCount++;
    }

    public void MarkCompleted()
    {
        Status = JobStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string errorMessage, TimeSpan? retryDelay = null)
    {
        ErrorMessage = errorMessage;
        if (TryCount < 3 && retryDelay.HasValue)
        {
            Status = JobStatus.Pending;
            NextTryAt = DateTime.UtcNow.Add(retryDelay.Value);
        }
        else
        {
            Status = JobStatus.Failed;
        }
    }
}
