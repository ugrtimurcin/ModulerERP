namespace ModulerERP.SharedKernel.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid TenantId { get; } // Useful if we extract tenant from token too
    bool IsAuthenticated { get; }
}
