using InteraktifKredi.Models;
using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace InteraktifKredi.Pages.Profile;

public class EsBilgileriModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<EsBilgileriModel> _logger;

    public EsBilgileriModel(IApiService apiService, ILogger<EsBilgileriModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [BindProperty]
    [Required(ErrorMessage = "Medeni durum seçimi gereklidir.")]
    public bool MaritalStatus { get; set; }

    [BindProperty]
    public bool WorkWife { get; set; }

    [BindProperty]
    [Range(0, 999999999.99, ErrorMessage = "Geçerli bir maaş miktarı giriniz.")]
    public decimal WifeSalaryAmount { get; set; }

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Session kontrolü
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var customerId))
        {
            _logger.LogWarning("Eş bilgileri sayfası: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        // Mevcut eş bilgilerini çek
        try
        {
            var response = await _apiService.GetWifeInfo(customerId);
            if (response.Success && response.Value != null)
            {
                var wifeInfo = response.Value;
                MaritalStatus = wifeInfo.MaritalStatus ?? false;
                WorkWife = wifeInfo.WorkWife ?? false;
                WifeSalaryAmount = wifeInfo.WifeSalaryAmount ?? 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eş bilgileri yükleme hatası");
            ErrorMessage = "Eş bilgileri yüklenirken bir hata oluştu.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Session kontrolü
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var customerId))
        {
            _logger.LogWarning("Eş bilgileri kaydetme: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        // Conditional validation
        if (MaritalStatus) // Evli ise
        {
            if (WorkWife && WifeSalaryAmount <= 0) // Çalışıyorsa maaş gereklidir
            {
                ModelState.AddModelError("WifeSalaryAmount", "Eş çalışıyorsa maaş bilgisi gereklidir.");
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var wifeInfoModel = new WifeInfoModel
            {
                CustomerId = customerId,
                MaritalStatus = MaritalStatus,
                WorkWife = WorkWife,
                WifeSalaryAmount = WorkWife ? WifeSalaryAmount : 0 // Çalışmıyorsa 0 (DECIMAL - ZORUNLU!)
            };

            var response = await _apiService.SaveWifeInfo(wifeInfoModel);

            if (response.Success)
            {
                _logger.LogInformation("Eş bilgileri kaydedildi: CustomerId={CustomerId}, MaritalStatus={MaritalStatus}, WorkWife={WorkWife}", 
                    customerId, MaritalStatus, WorkWife);
                SuccessMessage = "Eş bilgileriniz başarıyla kaydedildi.";
                return Page();
            }

            ErrorMessage = response.Message ?? "Eş bilgileri kaydedilemedi.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eş bilgileri kaydetme hatası");
            ErrorMessage = "Bir hata oluştu. Lütfen tekrar deneyin.";
            return Page();
        }
    }
}
