using ModulerERP.ProjectManagement.Application.DTOs;

namespace ModulerERP.ProjectManagement.Application.Interfaces;

public interface IProjectChangeOrderService
{
    Task<List<ProjectChangeOrderDto>> GetByProjectIdAsync(Guid tenantId, Guid projectId);
    Task<ProjectChangeOrderDto> CreateAsync(Guid tenantId, Guid userId, CreateChangeOrderDto dto);
    Task ApproveAsync(Guid tenantId, Guid userId, Guid changeOrderId);
    Task RejectAsync(Guid tenantId, Guid userId, Guid changeOrderId);
}
