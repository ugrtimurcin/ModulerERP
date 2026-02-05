namespace ModulerERP.ProjectManagement.Domain.Entities;

public class ProjectBudget
{
    public decimal TotalBudget { get; set; }
    public decimal MaterialBudget { get; set; }
    public decimal LaborBudget { get; set; }
    public decimal SubcontractorBudget { get; set; }
    public decimal ExpenseBudget { get; set; }
}
