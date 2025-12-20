using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages.GizlilikPolitikasi;

public class IndexModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IApiService apiService, ILogger<IndexModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public string? KvkkText { get; set; }
    public string? ErrorMessage { get; set; }
    private const int KVKK_ID = 1; // KVKK ID

    public async Task OnGetAsync()
    {
        // Public sayfa - login kontrolü yok
        // KVKK metnini API'den çek
        try
        {
            var response = await _apiService.GetKvkkText(KVKK_ID);
            if (response.Success && !string.IsNullOrEmpty(response.Value))
            {
                KvkkText = response.Value;
            }
            else
            {
                ErrorMessage = response.Message ?? "KVKK metni yüklenemedi.";
                KvkkText = "KVKK metni yüklenemedi. Lütfen sayfayı yenileyin.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KVKK metni yükleme hatası");
            ErrorMessage = "KVKK metni yüklenirken bir hata oluştu.";
            KvkkText = "KVKK metni yüklenemedi. Lütfen sayfayı yenileyin.";
        }
    }
}

