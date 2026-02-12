using ModulerERP.SharedKernel.Entities;

namespace ModulerERP.SystemCore.Domain.Entities;

public class ExchangeRate : BaseEntity
{
    public Guid SourceCurrencyId { get; set; }
    public Guid TargetCurrencyId { get; set; }
    public decimal Rate { get; set; }
    public DateTime Date { get; set; }
}
