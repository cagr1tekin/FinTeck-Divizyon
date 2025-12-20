using InteraktifKredi.Models;
using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace InteraktifKredi.Pages.Profile;

public class GelirBilgileriModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<GelirBilgileriModel> _logger;
    private readonly IWebHostEnvironment _environment;

    public GelirBilgileriModel(
        IApiService apiService, 
        ILogger<GelirBilgileriModel> logger,
        IWebHostEnvironment environment)
    {
        _apiService = apiService;
        _logger = logger;
        _environment = environment;
    }

    [BindProperty]
    [Required(ErrorMessage = "Aylık net maaş gereklidir.")]
    [Range(0, 999999999.99, ErrorMessage = "Geçerli bir maaş miktarı giriniz.")]
    public decimal SalaryAmount { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Maaş bankası seçimi gereklidir.")]
    public string SalaryBank { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Çalışma sektörü seçimi gereklidir.")]
    public int WorkSector { get; set; }

    [BindProperty]
    public bool CarStatus { get; set; }

    [BindProperty]
    public bool HouseStatus { get; set; }

    public List<SelectListItem> Banks { get; set; } = new();
    public List<SelectListItem> Sectors { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Session kontrolü
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var customerId))
        {
            _logger.LogWarning("Gelir bilgileri sayfası: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        // Banka ve sektör listelerini yükle
        LoadBanks();
        LoadSectors();

        // Mevcut finansal bilgileri çek
        try
        {
            var response = await _apiService.GetFinanceAssets(customerId);
            if (response.Success && response.Value != null)
            {
                var financeInfo = response.Value;
                SalaryAmount = financeInfo.SalaryAmount ?? 0;
                SalaryBank = financeInfo.SalaryBank ?? string.Empty;
                WorkSector = financeInfo.WorkSector;
                CarStatus = financeInfo.CarStatus ?? false;
                HouseStatus = financeInfo.HouseStatus ?? false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Finansal bilgiler yükleme hatası");
            ErrorMessage = "Finansal bilgiler yüklenirken bir hata oluştu.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Session kontrolü
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var customerId))
        {
            _logger.LogWarning("Gelir bilgileri kaydetme: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        // Listeleri tekrar yükle
        LoadBanks();
        LoadSectors();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var financeModel = new FinanceModel
            {
                CustomerId = customerId,
                SalaryAmount = SalaryAmount, // DECIMAL - ZORUNLU!
                SalaryBank = SalaryBank,
                WorkSector = WorkSector,
                CarStatus = CarStatus,
                HouseStatus = HouseStatus
            };

            var response = await _apiService.SaveFinanceAssets(financeModel);

            if (response.Success)
            {
                _logger.LogInformation("Finansal bilgiler kaydedildi: CustomerId={CustomerId}, SalaryAmount={SalaryAmount}", 
                    customerId, SalaryAmount);
                SuccessMessage = "Gelir bilgileriniz başarıyla kaydedildi.";
                return Page();
            }

            ErrorMessage = response.Message ?? "Gelir bilgileri kaydedilemedi.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gelir bilgileri kaydetme hatası");
            ErrorMessage = "Bir hata oluştu. Lütfen tekrar deneyin.";
            return Page();
        }
    }

    private void LoadBanks()
    {
        Banks = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Banka Seçiniz", Selected = string.IsNullOrEmpty(SalaryBank) }
        };

        // Türkiye'deki başlıca bankalar
        var banks = new[]
        {
            "Ziraat Bankası",
            "İş Bankası",
            "Garanti BBVA",
            "Akbank",
            "Yapı Kredi",
            "Halkbank",
            "Vakıfbank",
            "Denizbank",
            "QNB Finansbank",
            "TEB",
            "ING Bank",
            "HSBC",
            "Şekerbank",
            "Türkiye Finans",
            "Albaraka Türk",
            "Kuveyt Türk",
            "Ziraat Katılım",
            "Vakıf Katılım",
            "Türkiye Emlak Katılım",
            "Diğer"
        };

        foreach (var bank in banks)
        {
            Banks.Add(new SelectListItem
            {
                Value = bank,
                Text = bank,
                Selected = SalaryBank == bank
            });
        }
    }

    private void LoadSectors()
    {
        Sectors = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Sektör Seçiniz", Selected = WorkSector == 0 }
        };

        try
        {
            var jsonPath = System.IO.Path.Combine(_environment.WebRootPath, "data", "sektor_id.json");
            if (System.IO.File.Exists(jsonPath))
            {
                var jsonContent = System.IO.File.ReadAllText(jsonPath);
                var sectors = JsonSerializer.Deserialize<List<SectorItem>>(jsonContent);
                
                if (sectors != null)
                {
                    foreach (var sector in sectors)
                    {
                        Sectors.Add(new SelectListItem
                        {
                            Value = sector.id.ToString(),
                            Text = sector.name,
                            Selected = WorkSector == sector.id
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sektör listesi yükleme hatası");
        }
    }

    private class SectorItem
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
    }
}
