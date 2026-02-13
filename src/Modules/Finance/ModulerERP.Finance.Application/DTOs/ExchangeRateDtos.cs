namespace ModulerERP.Finance.Application.DTOs;

public class ExchangeRateDto
{
    public Guid Id { get; set; }
    public Guid FromCurrencyId { get; set; }
    public Guid ToCurrencyId { get; set; }
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyCode { get; set; } = string.Empty;
    public DateTime RateDate { get; set; }
    public decimal Rate { get; set; }
    public decimal BuyingRate { get; set; }
    public decimal SellingRate { get; set; }
    public string Source { get; set; } = string.Empty;
}

public class CreateExchangeRateDto
{
    public Guid FromCurrencyId { get; set; }
    public Guid ToCurrencyId { get; set; }
    public DateTime RateDate { get; set; }
    public decimal Rate { get; set; }
    public decimal BuyingRate { get; set; }
    public decimal SellingRate { get; set; }
}

public class UpdateExchangeRateDto
{
    public decimal Rate { get; set; }
    public decimal BuyingRate { get; set; }
    public decimal SellingRate { get; set; }
}
