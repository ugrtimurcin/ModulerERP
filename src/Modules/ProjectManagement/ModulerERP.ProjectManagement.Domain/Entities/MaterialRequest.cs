using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class MaterialRequest : BaseEntity
{
    public Guid ProjectId { get; set; }
    public int RequestNo { get; set; }
    public DateTime RequestDate { get; set; }
    
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Approved, Ordered, Delivered
    
    public ICollection<MaterialRequestItem> Items { get; set; } = new List<MaterialRequestItem>();
}

public class MaterialRequestItem : BaseEntity
{
    public Guid MaterialRequestId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public DateTime RequiredDate { get; set; }
}
