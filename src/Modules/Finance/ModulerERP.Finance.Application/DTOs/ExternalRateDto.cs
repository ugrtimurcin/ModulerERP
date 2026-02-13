namespace ModulerERP.Finance.Application.DTOs;

public class ExternalRateDto
{
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public decimal BuyingRate { get; set; }
    public decimal SellingRate { get; set; }
}
