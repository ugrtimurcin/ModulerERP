using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.HR.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ManagerId { get; private set; }
    public Guid? ParentDepartmentId { get; private set; }
    public Guid? SgkRiskProfileId { get; private set; }

    // Navigation
    public Employee? Manager { get; private set; }
    public Department? ParentDepartment { get; private set; }
    public SgkRiskProfile? SgkRiskProfile { get; private set; }
    public ICollection<Department> ChildDepartments { get; private set; } = new List<Department>();

    private Department() { }

    public static Department Create(Guid tenantId, string name, Guid createdBy, string? description = null, Guid? managerId = null, Guid? parentDepartmentId = null, Guid? sgkRiskProfileId = null)
    {
        var dept = new Department
        {
            Name = name,
            Description = description,
            ManagerId = managerId,
            ParentDepartmentId = parentDepartmentId,
            SgkRiskProfileId = sgkRiskProfileId
        };
        dept.SetTenant(tenantId);
        dept.SetCreator(createdBy);
        return dept;
    }

    public void Update(string name, string? description, Guid? managerId, Guid? parentDepartmentId, Guid? sgkRiskProfileId)
    {
        Name = name;
        Description = description;
        ManagerId = managerId;
        ParentDepartmentId = parentDepartmentId;
        SgkRiskProfileId = sgkRiskProfileId;
    }
}
