using ModulerERP.SharedKernel.Entities;


namespace ModulerERP.CRM.Domain.Entities;

public class SaleAgent : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    // Note: Employee is in a different module/schema (HR), so we might simply store the ID.
    // However, if we want cross-module navigation at DB level (monolith advantage), we can include it.
    // For strict modularity, we might just keep the ID. 
    // Given the project structure is a Modular Monolith with shared DB, we can use navigation 
    // BUT EF Core usually prefers DbContexts to be bounded.
    // If CRMDbContext doesn't include Employee, we can't have a navigation property to it effectively 
    // unless we map it (which blurs boundaries).
    // Reviewing 'db_structure.md', SaleAgent links to Employee.
    // DECISION: Store EmployeeId. Navigation property will be omitted to strictly respect Bounded Contexts.
    // Queries needing Employee names will be done via a service or specialized query.
    
    public decimal CommissionRate { get; private set; }
    public string CommissionType { get; private set; } = "Percentage"; // Percentage or Fixed

    private SaleAgent() { }

    public static SaleAgent Create(Guid tenantId, Guid employeeId, decimal commissionRate, Guid createdByUserId)
    {
        var agent = new SaleAgent
        {
            EmployeeId = employeeId,
            CommissionRate = commissionRate
        };
        agent.SetTenant(tenantId);
        agent.SetCreator(createdByUserId);
        return agent;
    }
}
