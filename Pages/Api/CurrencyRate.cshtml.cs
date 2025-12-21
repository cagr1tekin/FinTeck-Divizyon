using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages.Api;

public class CurrencyRateModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<CurrencyRateModel> _logger;
    
    public CurrencyRateModel(IApiService apiService, ILogger<CurrencyRateModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }
    
    public async Task<IActionResult> OnGetAsync(string currencyCode = "USD")
    {
        if (string.IsNullOrEmpty(currencyCode))
        {
            return new JsonResult(new { success = false, message = "Currency code gerekli" });
        }
        
        try
        {
            var response = await _apiService.GetCurrencyRate(currencyCode);
            return new JsonResult(new { 
                success = response.Success, 
                data = response.Data,
                message = response.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Currency rate API hatası: CurrencyCode={CurrencyCode}", currencyCode);
            return new JsonResult(new { 
                success = false, 
                message = "Bir hata oluştu" 
            });
        }
    }
}

