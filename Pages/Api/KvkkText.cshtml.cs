using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages.Api;

[IgnoreAntiforgeryToken]
public class KvkkTextModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<KvkkTextModel> _logger;

    public KvkkTextModel(IApiService apiService, ILogger<KvkkTextModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; } = 2;

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var response = await _apiService.GetKvkkText(Id);
            
            if (response.Success)
            {
                return new JsonResult(new
                {
                    success = true,
                    value = response.Value
                });
            }

            return new JsonResult(new
            {
                success = false,
                message = response.Message ?? "KVKK metni alınamadı."
            })
            {
                StatusCode = response.StatusCode > 0 ? response.StatusCode : 400
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

