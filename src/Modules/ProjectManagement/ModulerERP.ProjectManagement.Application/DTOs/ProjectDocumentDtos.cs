namespace ModulerERP.ProjectManagement.Application.DTOs;

public record ProjectDocumentDto(
    Guid Id,
    Guid ProjectId,
    string Title,
    string DocumentType,
    string FileUrl,
    Guid? SystemFileId,
    string Description,
    DateTime UploadedAt // CreatedAt
);

public record CreateProjectDocumentDto(
    Guid ProjectId,
    string Title,
    string DocumentType,
    string FileUrl,
    Guid? SystemFileId,
    string Description
);
