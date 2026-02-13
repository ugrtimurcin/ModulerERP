using System.Xml.Linq;
using ModulerERP.Finance.Application.Interfaces;
using ModulerERP.SharedKernel.Results;
using System.Net.Http;
using System.Globalization;

namespace ModulerERP.Finance.Infrastructure.Services;

public class KktcRateProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;

    public KktcRateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<List<ModulerERP.Finance.Application.DTOs.ExternalRateDto>>> GetDailyRatesAsync(DateTime? date = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Official URL: https://mb.gov.ct.tr/kur/gunluk.xml
            // Historical: https://mb.gov.ct.tr/kur/tarih/ddMMyyyy.xml
            string url;
            if (date.HasValue && date.Value.Date != DateTime.Today)
            {
                url = $"https://mb.gov.ct.tr/kur/tarih/{date.Value:ddMMyyyy}.xml";
            }
            else
            {
                url = "https://mb.gov.ct.tr/kur/gunluk.xml";
            }
            
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var responseMsg = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            responseMsg.EnsureSuccessStatusCode();

            // The KKTC API might return an invalid charset in Content-Type (e.g. ISO-8859-9 but header says something else or is malformed).
            // We read bytes and interpret them.
            // Turkish sites often use ISO-8859-9 or Windows-1254.
            // We'll trust the bytes more than the header.
            
            var bytes = await responseMsg.Content.ReadAsByteArrayAsync(cancellationToken);
            
            // Register provider for code pages if not already done (needed for 1254)
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // Attempt to decode. If utf-8 fails, try 1254.
            // Actually, XML decl usually says encoding.
            // Let's assume 1254 (Turkish) if not standard.
            // string response;
            // try
            // {
            //     // Try UTF-8 first
            //      response = System.Text.Encoding.UTF8.GetString(bytes);
            // }
            // catch
            // {
            //      // Fallback
            //      response = System.Text.Encoding.GetEncoding(1254).GetString(bytes);
            // }
            
            // If the XML starts with weird chars or encoding declaration mismatch, we might need to handle it.
            // Better approach: Let StreamReader detect or force 1254.
            // Given the error "Invalid character set", the header is definitely bad.
            // Let's force Windows-1254 which covers Turkish.
            //  response = System.Text.Encoding.GetEncoding(1254).GetString(bytes);

            // If it's actually UTF-8 but simple ASCII chars, 1254 typically maps 1:1 for ASCII.
            // If it has a BOM, we might need to be careful.
            
            // Refined:
            // Use StreamReader with auto-detect, but if it fails, fallback.
            // Actually, simpler: The error was Invalid Character Set in HEADER.
            // Reading bytes bypasses header validation.
            // We'll try to parse as 1254 as it's the most robust for legacy TR sites.
            
             // Force Windows-1254 for Turkish characters
            string response = System.Text.Encoding.GetEncoding("windows-1254").GetString(bytes);
            
            var xdoc = XDocument.Parse(response);
            var rates = new List<ModulerERP.Finance.Application.DTOs.ExternalRateDto>();

            // Parse XML
            /*
             <Resmi_Kur>
               <Sembol>USD</Sembol>
               <Doviz_Satis>43.41950</Doviz_Satis>
               <Doviz_Alis>43.10000</Doviz_Alis>
             </Resmi_Kur>
            */

            foreach (var kur in xdoc.Descendants("Resmi_Kur"))
            {
                var code = kur.Element("Sembol")?.Value;
                var satisStr = kur.Element("Doviz_Satis")?.Value;
                var alisStr = kur.Element("Doviz_Alis")?.Value;

                if (!string.IsNullOrEmpty(code) && 
                    !string.IsNullOrEmpty(satisStr))
                {
                    decimal.TryParse(satisStr, NumberStyles.Any, new CultureInfo("en-US"), out var sellingRate);
                    decimal.TryParse(alisStr, NumberStyles.Any, new CultureInfo("en-US"), out var buyingRate);

                    if (sellingRate > 0)
                    {
                        rates.Add(new ModulerERP.Finance.Application.DTOs.ExternalRateDto
                        {
                            CurrencyCode = code,
                            Rate = sellingRate,       // Standard Rate = Selling Rate
                            SellingRate = sellingRate,
                            BuyingRate = buyingRate > 0 ? buyingRate : sellingRate // Fallback if buying missing
                        });
                    }
                }
            }

            return Result<List<ModulerERP.Finance.Application.DTOs.ExternalRateDto>>.Success(rates);
        }
        catch (Exception ex)
        {
            return Result<List<ModulerERP.Finance.Application.DTOs.ExternalRateDto>>.Failure($"Failed to fetch KKTC rates: {ex.Message}");
        }
    }
}
