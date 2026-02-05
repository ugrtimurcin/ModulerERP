using ModulerERP.Finance.Domain.Enums;

namespace ModulerERP.Finance.Application.DTOs;

public class FiscalPeriodDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int FiscalYear { get; set; }
    public int PeriodNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsAdjustment { get; set; }
}

public class CreateFiscalPeriodDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int FiscalYear { get; set; }
    public int PeriodNumber { get; set; }
    public bool IsAdjustment { get; set; }
}

public class UpdateFiscalPeriodDto
{
    public string Name { get; set; } = string.Empty;
    public PeriodStatus Status { get; set; }
}
