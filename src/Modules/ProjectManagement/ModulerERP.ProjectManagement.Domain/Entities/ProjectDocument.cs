using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class ProjectDocument : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty; // Contract, Blueprint, Photo
    public string FileUrl { get; set; } = string.Empty; // Link to SystemCore.MediaFile if needed
    public Guid? SystemFileId { get; set; } // Reference to SystemCore.MediaFile
    public string Description { get; set; } = string.Empty;
}
