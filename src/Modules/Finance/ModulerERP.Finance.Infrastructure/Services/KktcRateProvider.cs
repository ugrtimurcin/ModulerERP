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

    public async Task<Result<List<(string CurrencyCode, decimal Rate)>>> GetDailyRatesAsync(DateTime? date = null, CancellationToken cancellationToken = default)
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
            string response;
            try
            {
                // Try UTF-8 first
                 response = System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                 // Fallback
                 response = System.Text.Encoding.GetEncoding(1254).GetString(bytes);
            }
            
            // If the XML starts with weird chars or encoding declaration mismatch, we might need to handle it.
            // Better approach: Let StreamReader detect or force 1254.
            // Given the error "Invalid character set", the header is definitely bad.
            // Let's force Windows-1254 which covers Turkish.
             response = System.Text.Encoding.GetEncoding(1254).GetString(bytes);

            // If it's actually UTF-8 but simple ASCII chars, 1254 typically maps 1:1 for ASCII.
            // If it has a BOM, we might need to be careful.
            
            // Refined:
            // Use StreamReader with auto-detect, but if it fails, fallback.
            // Actually, simpler: The error was Invalid Character Set in HEADER.
            // Reading bytes bypasses header validation.
            // We'll try to parse as 1254 as it's the most robust for legacy TR sites.
            
             response = System.Text.Encoding.GetEncoding("windows-1254").GetString(bytes);
            
            var xdoc = XDocument.Parse(response);
            var rates = new List<(string, decimal)>();

            // Parse XML
            /*
             <Resmi_Kur>
               <Sembol>USD</Sembol>
               <Doviz_Satis>43.41950</Doviz_Satis>
             </Resmi_Kur>
            */

            foreach (var kur in xdoc.Descendants("Resmi_Kur"))
            {
                var code = kur.Element("Sembol")?.Value;
                var rateStr = kur.Element("Doviz_Satis")?.Value;

                if (!string.IsNullOrEmpty(code) && 
                    !string.IsNullOrEmpty(rateStr) && 
                    decimal.TryParse(rateStr, NumberStyles.Any, new CultureInfo("en-US"), out var rate)) // XML usually uses dot decimal
                {
                    // Check logic: 
                    // <Doviz_Satis> is usually the Bank's selling rate.
                    // Depending on business need, we might use Alis or Satis. Using Satis (Selling) as conservative default for costs.
                    // Also 'Efektif' is for cash. 'Doviz' is for account transfers. Usually 'Doviz Satis' is standard.
                    
                    rates.Add((code, rate));
                }
            }

            return Result<List<(string, decimal)>>.Success(rates);
        }
        catch (Exception ex)
        {
            return Result<List<(string, decimal)>>.Failure($"Failed to fetch KKTC rates: {ex.Message}");
        }
    }
}
