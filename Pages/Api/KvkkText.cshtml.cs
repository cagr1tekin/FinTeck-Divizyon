using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace InteraktifKredi.Pages.Api;

[IgnoreAntiforgeryToken]
public class KvkkTextModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KvkkTextModel> _logger;

    public KvkkTextModel(IConfiguration configuration, ILogger<KvkkTextModel> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            // Configuration'dan API code'u al
            var apiCode = _configuration.GetValue<string>("ApiSettings:IdcApiKvkkTextCode") ?? 
                         "50aiAx0i6mmwXV4gPTcKiHc9plIMg3s6Kcer667-40K1AzFulZ-Mkw==";
            var idcApiUrl = _configuration.GetValue<string>("ApiSettings:IdcApiUrl") ?? 
                           "https://api-idc.azurewebsites.net";
            
            var url = $"{idcApiUrl}/api/kvkk/text/{Id}?code={Uri.EscapeDataString(apiCode)}";
            
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            var response = await httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    // Response'u parse et - Id, Name, Text alanları var
                    var kvkkData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    if (kvkkData != null)
                    {
                        var kvkkText = kvkkData.Text?.ToString() ?? string.Empty;
                        var kvkkName = kvkkData.Name?.ToString() ?? "KVKK Aydınlatma Metni";
                        
                        return new JsonResult(new
                        {
                            success = true,
                            value = kvkkText,
                            name = kvkkName
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "KVKK response parse hatası: Response={Response}", responseContent);
                }
                
                // Fallback: Eğer parse edilemezse direkt text kullan
                return new JsonResult(new
                {
                    success = true,
                    value = responseContent,
                    name = "KVKK Aydınlatma Metni"
                });
            }
            
            _logger.LogWarning("KVKK metni alınamadı: StatusCode={StatusCode}, Response={Response}", 
                response.StatusCode, responseContent);
            
            return new JsonResult(new
            {
                success = false,
                message = "KVKK metni alınamadı."
            })
            {
                StatusCode = (int)response.StatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KVKK metni API hatası: KvkkId={KvkkId}", Id);
            return new JsonResult(new
            {
                success = false,
                message = "Bir hata oluştu."
            })
            {
                StatusCode = 500
            };
        }
    }
}

