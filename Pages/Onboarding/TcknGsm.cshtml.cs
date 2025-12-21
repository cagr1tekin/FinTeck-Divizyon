using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace InteraktifKredi.Pages.Onboarding;

public class TcknGsmModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<TcknGsmModel> _logger;

    public TcknGsmModel(IApiService apiService, ILogger<TcknGsmModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [BindProperty]
    [Required(ErrorMessage = "TC Kimlik No gereklidir.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "TC Kimlik No sadece rakam içermelidir.")]
    public string Tckn { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "GSM numarası gereklidir.")]
    [StringLength(11, MinimumLength = 10, ErrorMessage = "GSM numarası 10-11 haneli olmalıdır.")]
    [RegularExpression(@"^5\d{9,10}$", ErrorMessage = "GSM numarası 5 ile başlamalı ve 10-11 haneli olmalıdır.")]
    public string Gsm { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // API çağrısı - TCKN ve GSM doğrulama
            var response = await _apiService.ValidateTcknGsm(Tckn, Gsm);

            if (response.Success && response.Value != null)
            {
                // Başarılı - CustomerId'yi session'a kaydet
                HttpContext.Session.SetString("CustomerId", response.Value.CustomerId.ToString());
                HttpContext.Session.SetString("TCKN", Tckn);
                HttpContext.Session.SetString("GSM", Gsm);
                
                // Müşteri adını session'a kaydet (Name ve Surname birleştirilerek)
                var customerName = string.Empty;
                if (!string.IsNullOrEmpty(response.Value.Name) || !string.IsNullOrEmpty(response.Value.Surname))
                {
                    var nameParts = new List<string>();
                    if (!string.IsNullOrEmpty(response.Value.Name))
                        nameParts.Add(response.Value.Name);
                    if (!string.IsNullOrEmpty(response.Value.Surname))
                        nameParts.Add(response.Value.Surname);
                    customerName = string.Join(" ", nameParts);
                }
                
                // Eğer ad soyad yoksa varsayılan değer kullan
                if (string.IsNullOrEmpty(customerName))
                {
                    customerName = "Müşteri";
                }
                
                HttpContext.Session.SetString("CustomerName", customerName);

                _logger.LogInformation("TCKN-GSM doğrulama başarılı: CustomerId={CustomerId}, CustomerName={CustomerName}", 
                    response.Value.CustomerId, customerName);

                // OTP oluştur ve SMS gönder
                try
                {
                    var otpResponse = await _apiService.GenerateOtp(Tckn, Gsm);
                    if (otpResponse.Success && otpResponse.Value != null)
                    {
                        var smsResponse = await _apiService.SendOtpSms(Gsm, otpResponse.Value.OtpCode);
                        if (!smsResponse.Success)
                        {
                            _logger.LogWarning("OTP SMS gönderilemedi: GSM={MaskedGsm}", 
                                Gsm.Substring(0, 3) + "****" + Gsm.Substring(7));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OTP oluşturma/gönderme hatası");
                    // OTP hatası olsa bile sayfaya yönlendir, kullanıcı tekrar gönderebilir
                }

                // OTP sayfasına yönlendir
                return RedirectToPage("/Onboarding/OtpDogrula", new { tckn = Tckn, gsm = Gsm });
            }

            // Hata durumu
            ErrorMessage = response.Message ?? "Doğrulama başarısız. Lütfen bilgilerinizi kontrol edin.";
            ModelState.AddModelError("", ErrorMessage);
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TCKN-GSM doğrulama hatası");
            ErrorMessage = "Bir hata oluştu. Lütfen tekrar deneyin.";
            ModelState.AddModelError("", ErrorMessage);
            return Page();
        }
    }
}
