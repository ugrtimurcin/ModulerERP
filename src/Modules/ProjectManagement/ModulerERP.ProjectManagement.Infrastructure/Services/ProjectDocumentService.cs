using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Application.DTOs;
using ModulerERP.ProjectManagement.Application.Interfaces;
using ModulerERP.ProjectManagement.Domain.Entities;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;

namespace ModulerERP.ProjectManagement.Infrastructure.Services;

public class ProjectDocumentService : IProjectDocumentService
{
    private readonly ProjectManagementDbContext _context;

    public ProjectDocumentService(ProjectManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectDocumentDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId)
    {
        return await _context.ProjectDocuments
            .Where(x => x.TenantId == tenantId && x.ProjectId == projectId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ProjectDocumentDto(
                x.Id,
                x.ProjectId,
                x.Title,
                x.DocumentType,
                x.FileUrl,
                x.SystemFileId,
                x.Description,
                x.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<ProjectDocumentDto> CreateAsync(Guid tenantId, Guid userId, CreateProjectDocumentDto dto)
    {
        var doc = new ProjectDocument
        {
            ProjectId = dto.ProjectId,
            Title = dto.Title,
            DocumentType = dto.DocumentType,
            FileUrl = dto.FileUrl,
            SystemFileId = dto.SystemFileId,
            Description = dto.Description
        };

        doc.SetTenant(tenantId);
        doc.SetCreator(userId);

        _context.ProjectDocuments.Add(doc);
        await _context.SaveChangesAsync();

        return new ProjectDocumentDto(
            doc.Id, doc.ProjectId, doc.Title, doc.DocumentType, doc.FileUrl, 
            doc.SystemFileId, doc.Description, doc.CreatedAt);
    }
    public async Task DeleteAsync(Guid tenantId, Guid userId, Guid documentId)
    {
        var doc = await _context.ProjectDocuments
            .FirstOrDefaultAsync(x => x.Id == documentId && x.TenantId == tenantId && !x.IsDeleted);

        if (doc == null) throw new KeyNotFoundException($"Document {documentId} not found.");

        doc.Delete(userId);
        
        await _context.SaveChangesAsync();
    }
}
