using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.ProjectManagement.Domain.Entities;

public class SubcontractorContract : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid SubcontractorId { get; set; } // Link to Contact/Vendor
    
    public string ReferenceNo { get; set; } = string.Empty;
    public string ScopeOfWork { get; set; } = string.Empty;
    
    public decimal ContractAmount { get; set; }
    public Guid CurrencyId { get; set; }
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
