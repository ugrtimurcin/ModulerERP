using ModulerERP.ProjectManagement.Application.DTOs;

namespace ModulerERP.ProjectManagement.Application.Interfaces;

public interface IProgressPaymentService
{
    Task<List<ProgressPaymentDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId);
    Task<ProgressPaymentDto> CreateAsync(Guid tenantId, Guid userId, CreateProgressPaymentDto dto);
    Task ApproveAsync(Guid tenantId, Guid id); // Triggers Invoice/Financial integration
    Task UpdateDetailAsync(Guid tenantId, Guid paymentId, UpdateProgressPaymentDetailDto dto);
}
