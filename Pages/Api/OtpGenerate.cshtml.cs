using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages.Api;

[IgnoreAntiforgeryToken]
public class OtpGenerateModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<OtpGenerateModel> _logger;

    public OtpGenerateModel(IApiService apiService, ILogger<OtpGenerateModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public class OtpGenerateRequest
    {
        public string TCKN { get; set; } = string.Empty;
        public string GSM { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync([FromBody] OtpGenerateRequest request)
    {
        if (string.IsNullOrEmpty(request.TCKN) || string.IsNullOrEmpty(request.GSM))
        {
            return new JsonResult(new
            {
                success = false,
                message = "TCKN ve GSM gereklidir."
            })
            {
                StatusCode = 400
            };
        }

        try
        {
            var response = await _apiService.GenerateOtp(request.TCKN, request.GSM);

            if (response.Success)
            {
                return new JsonResult(new
                {
                    success = true,
                    value = new
                    {
                        otpCode = response.Value?.OtpCode,
                        expiresAt = response.Value?.ExpiresAt,
                        retryCount = response.Value?.RetryCount
                    }
                });
            }

            return new JsonResult(new
            {
                success = false,
                message = response.Message ?? "OTP oluşturulamadı."
            })
            {
                StatusCode = response.StatusCode > 0 ? response.StatusCode : 400
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OTP oluşturma API hatası");
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

