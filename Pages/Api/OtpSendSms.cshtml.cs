using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages.Api;

[IgnoreAntiforgeryToken]
public class OtpSendSmsModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<OtpSendSmsModel> _logger;

    public OtpSendSmsModel(IApiService apiService, ILogger<OtpSendSmsModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public class OtpSendSmsRequest
    {
        public string GSM { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync([FromBody] OtpSendSmsRequest request)
    {
        if (string.IsNullOrEmpty(request.GSM) || string.IsNullOrEmpty(request.OtpCode))
        {
            return new JsonResult(new
            {
                success = false,
                message = "GSM ve OtpCode gereklidir."
            })
            {
                StatusCode = 400
            };
        }

        try
        {
            var response = await _apiService.SendOtpSms(request.GSM, request.OtpCode);

            if (response.Success)
            {
                return new JsonResult(new
                {
                    success = true
                });
            }

            return new JsonResult(new
            {
                success = false,
                message = response.Message ?? "SMS gönderilemedi."
            })
            {
                StatusCode = response.StatusCode > 0 ? response.StatusCode : 400
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OTP SMS gönderme API hatası");
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

