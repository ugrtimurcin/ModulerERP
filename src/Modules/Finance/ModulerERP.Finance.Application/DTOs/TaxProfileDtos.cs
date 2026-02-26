using System;

namespace ModulerERP.Finance.Application.DTOs;

public class TaxProfileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal VatRate { get; set; }
    public decimal WithholdingRate { get; set; }
    public decimal StampDutyRate { get; set; }
    public Guid? VatAccountId { get; set; }
    public Guid? WithholdingAccountId { get; set; }
    public Guid? StampDutyAccountId { get; set; }
    public bool IsActive { get; set; }
}

public class CreateTaxProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal VatRate { get; set; }
    public decimal WithholdingRate { get; set; }
    public decimal StampDutyRate { get; set; }
    public Guid? VatAccountId { get; set; }
    public Guid? WithholdingAccountId { get; set; }
    public Guid? StampDutyAccountId { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateTaxProfileDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal VatRate { get; set; }
    public decimal WithholdingRate { get; set; }
    public decimal StampDutyRate { get; set; }
    public Guid? VatAccountId { get; set; }
    public Guid? WithholdingAccountId { get; set; }
    public Guid? StampDutyAccountId { get; set; }
    public bool IsActive { get; set; }
}
