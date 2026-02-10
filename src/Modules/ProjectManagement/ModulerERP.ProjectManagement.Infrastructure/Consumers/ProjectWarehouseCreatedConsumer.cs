using MediatR;
using Microsoft.EntityFrameworkCore;
using ModulerERP.ProjectManagement.Infrastructure.Persistence;
using ModulerERP.SharedKernel.IntegrationEvents;

namespace ModulerERP.ProjectManagement.Infrastructure.Consumers;

public class ProjectWarehouseCreatedConsumer : INotificationHandler<ProjectWarehouseCreatedEvent>
{
    private readonly ProjectManagementDbContext _context;

    public ProjectWarehouseCreatedConsumer(ProjectManagementDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ProjectWarehouseCreatedEvent message, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == message.ProjectId && p.TenantId == message.TenantId, cancellationToken);

        if (project == null)
        {
            // Log warning
            return;
        }

        project.VirtualWarehouseId = message.WarehouseId;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
