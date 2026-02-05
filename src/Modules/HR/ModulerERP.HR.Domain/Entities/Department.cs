using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ManagerId { get; private set; }

    // Navigation
    public Employee? Manager { get; private set; }

    private Department() { }

    public static Department Create(Guid tenantId, string name, Guid createdBy, string? description = null, Guid? managerId = null)
    {
        var dept = new Department
        {
            Name = name,
            Description = description,
            ManagerId = managerId
        };
        dept.SetTenant(tenantId);
        dept.SetCreator(createdBy);
        return dept;
    }

    public void Update(string name, string? description, Guid? managerId)
    {
        Name = name;
        Description = description;
        ManagerId = managerId;
    }
}
