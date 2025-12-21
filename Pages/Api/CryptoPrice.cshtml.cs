using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages.Api;

public class CryptoPriceModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<CryptoPriceModel> _logger;
    
    public CryptoPriceModel(IApiService apiService, ILogger<CryptoPriceModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }
    
    public async Task<IActionResult> OnGetAsync(string symbols = "BTC,ETH,BNB,SOL", string convert = "TRY")
    {
        try
        {
            var response = await _apiService.GetCryptoCurrencyPrices(symbols, convert);
            return new JsonResult(new { 
                success = response.Success, 
                data = response.Data,
                message = response.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Crypto price API hatası");
            return new JsonResult(new { 
                success = false, 
                message = "Bir hata oluştu" 
            });
        }
    }
}

