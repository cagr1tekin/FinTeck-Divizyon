using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace InteraktifKredi.Pages.Onboarding;

public class KvkkOnayModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<KvkkOnayModel> _logger;

    public KvkkOnayModel(IApiService apiService, ILogger<KvkkOnayModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [BindProperty]
    [Required(ErrorMessage = "KVKK onayı gereklidir.")]
    [MustBeTrue(ErrorMessage = "KVKK aydınlatma metnini kabul etmelisiniz.")]
    public bool KvkkAccepted { get; set; }

    public string? KvkkText { get; set; }
    public string? ErrorMessage { get; set; }
    private const int KVKK_ID = 1; // Varsayılan KVKK ID

    public async Task<IActionResult> OnGetAsync()
    {
        // Session'dan CustomerId'yi al
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr))
        {
            _logger.LogWarning("KVKK sayfasına erişim: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

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

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            // ModelState geçersizse KVKK metnini tekrar yükle
            await OnGetAsync();
            return Page();
        }

        // Session'dan CustomerId'yi al
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var customerId))
        {
            _logger.LogWarning("KVKK onayı: Session'da CustomerId yok");
            ErrorMessage = "Oturum bilgisi bulunamadı. Lütfen tekrar giriş yapın.";
            await OnGetAsync();
            return Page();
        }

        try
        {
            // KVKK onayını API'ye kaydet
            var response = await _apiService.SaveKvkkOnay(KVKK_ID, customerId);

            if (response.Success)
            {
                _logger.LogInformation("KVKK onayı kaydedildi: CustomerId={CustomerId}, KvkkId={KvkkId}", 
                    customerId, KVKK_ID);

                // Başarılı - Dashboard'a yönlendir
                return RedirectToPage("/Dashboard/Index");
            }

            ErrorMessage = response.Message ?? "KVKK onayı kaydedilemedi.";
            await OnGetAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KVKK onayı kaydetme hatası: CustomerId={CustomerId}", customerId);
            ErrorMessage = "Bir hata oluştu. Lütfen tekrar deneyin.";
            await OnGetAsync();
            return Page();
        }
    }
}

// Custom validation attribute
public class MustBeTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        return value is bool boolValue && boolValue;
    }
}
