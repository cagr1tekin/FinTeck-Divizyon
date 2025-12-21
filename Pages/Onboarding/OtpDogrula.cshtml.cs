using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace InteraktifKredi.Pages.Onboarding;

public class OtpDogrulaModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<OtpDogrulaModel> _logger;

    public OtpDogrulaModel(IApiService apiService, ILogger<OtpDogrulaModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [BindProperty]
    [Required(ErrorMessage = "OTP kodu gereklidir.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP kodu 6 haneli olmalıdır.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "OTP kodu sadece rakam içermelidir.")]
    public string OtpCode { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? Tckn { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Gsm { get; set; }

    public string? MaskedGsm { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }

    public void OnGet()
    {
        // Session'dan bilgileri al
        Tckn = HttpContext.Session.GetString("TCKN");
        Gsm = HttpContext.Session.GetString("GSM");

        // Eğer session'da yoksa query string'den al
        if (string.IsNullOrEmpty(Tckn) && !string.IsNullOrEmpty(Request.Query["tckn"]))
        {
            Tckn = Request.Query["tckn"].ToString();
            if (!string.IsNullOrEmpty(Tckn))
            {
                HttpContext.Session.SetString("TCKN", Tckn);
            }
        }

        if (string.IsNullOrEmpty(Gsm) && !string.IsNullOrEmpty(Request.Query["gsm"]))
        {
            Gsm = Request.Query["gsm"].ToString();
            if (!string.IsNullOrEmpty(Gsm))
            {
                HttpContext.Session.SetString("GSM", Gsm);
            }
        }

        // GSM maskele
        if (!string.IsNullOrEmpty(Gsm) && Gsm.Length >= 10)
        {
            MaskedGsm = Gsm.Substring(0, 3) + " " + 
                       Gsm.Substring(3, 3) + " " + 
                       Gsm.Substring(6, 2) + " " + 
                       Gsm.Substring(8);
        }
        else
        {
            MaskedGsm = null;
        }

        // Session'dan retry count'u al
        var retryCountStr = HttpContext.Session.GetString("OtpRetryCount");
        if (!string.IsNullOrEmpty(retryCountStr) && int.TryParse(retryCountStr, out var retryCount))
        {
            RetryCount = retryCount;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Session'dan bilgileri al
        Tckn = HttpContext.Session.GetString("TCKN");
        Gsm = HttpContext.Session.GetString("GSM");

        if (string.IsNullOrEmpty(Tckn) || string.IsNullOrEmpty(Gsm))
        {
            _logger.LogWarning("OTP doğrulama: Session'da TCKN veya GSM yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        // GSM maskele
        if (!string.IsNullOrEmpty(Gsm) && Gsm.Length >= 10)
        {
            MaskedGsm = Gsm.Substring(0, 3) + " " + 
                       Gsm.Substring(3, 3) + " " + 
                       Gsm.Substring(6, 2) + " " + 
                       Gsm.Substring(8);
        }
        else
        {
            MaskedGsm = null;
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // OTP doğrulama API çağrısı
            var response = await _apiService.VerifyOtp(OtpCode);

            if (response.Success && response.Value != null)
            {
                // Başarılı - Token'ı session'a kaydet
                if (!string.IsNullOrEmpty(response.Value.Token))
                {
                    HttpContext.Session.SetString("AuthToken", response.Value.Token);
                }
                
                // CustomerId'yi token'dan parse et veya response'dan al
                var customerId = response.Value.CustomerId;
                
                // Eğer CustomerId 0 ise, session'dan al (TcknGsm'den kaydedilmiş olabilir)
                if (customerId == 0)
                {
                    var customerIdStr = HttpContext.Session.GetString("CustomerId");
                    if (!string.IsNullOrEmpty(customerIdStr) && long.TryParse(customerIdStr, out var sessionCustomerId))
                    {
                        customerId = sessionCustomerId;
                    }
                }
                
                HttpContext.Session.SetString("CustomerId", customerId.ToString());
                HttpContext.Session.Remove("OtpRetryCount"); // Retry count'u temizle
                
                // Eğer CustomerName session'da yoksa varsayılan değer koy
                var customerName = HttpContext.Session.GetString("CustomerName");
                if (string.IsNullOrEmpty(customerName))
                {
                    customerName = "Müşteri";
                    HttpContext.Session.SetString("CustomerName", customerName);
                }

                _logger.LogInformation("OTP doğrulama başarılı: CustomerId={CustomerId}, CustomerName={CustomerName}", 
                    customerId, customerName);

                // Başarılı - Direkt Dashboard'a yönlendir
                return RedirectToPage("/Dashboard/Index");
            }

            // Hata durumu - Retry count'u artır
            RetryCount = int.Parse(HttpContext.Session.GetString("OtpRetryCount") ?? "0") + 1;
            HttpContext.Session.SetString("OtpRetryCount", RetryCount.ToString());

            ErrorMessage = response.Message ?? "OTP kodu geçersiz. Lütfen tekrar deneyin.";
            
            if (RetryCount >= 3)
            {
                ErrorMessage = "3 kez yanlış kod girdiniz. Lütfen yeni kod talep edin.";
            }

            ModelState.AddModelError("", ErrorMessage);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OTP doğrulama hatası");
            ErrorMessage = "Bir hata oluştu. Lütfen tekrar deneyin.";
            ModelState.AddModelError("", ErrorMessage);
            return Page();
        }
    }
}
