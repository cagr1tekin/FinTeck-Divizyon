using InteraktifKredi.Models;
using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace InteraktifKredi.Pages.Profile;

public class MeslekBilgileriModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<MeslekBilgileriModel> _logger;
    private readonly IWebHostEnvironment _environment;

    public MeslekBilgileriModel(
        IApiService apiService, 
        ILogger<MeslekBilgileriModel> logger,
        IWebHostEnvironment environment)
    {
        _apiService = apiService;
        _logger = logger;
        _environment = environment;
    }

    [BindProperty]
    [Required(ErrorMessage = "Çalışma durumu seçimi gereklidir.")]
    public int WorkStatus { get; set; }

    [BindProperty]
    public int? JobId { get; set; }

    [BindProperty]
    public int? SectorId { get; set; }

    [BindProperty]
    [StringLength(200, ErrorMessage = "Firma adı en fazla 200 karakter olabilir.")]
    public string? CompanyName { get; set; }

    [BindProperty]
    [StringLength(100, ErrorMessage = "Pozisyon en fazla 100 karakter olabilir.")]
    public string? Position { get; set; }

    [BindProperty]
    [Range(0, 50, ErrorMessage = "Çalışma yılı 0-50 arasında olmalıdır.")]
    public int? WorkingYears { get; set; }

    [BindProperty]
    [Range(0, 11, ErrorMessage = "Çalışma ayı 0-11 arasında olmalıdır.")]
    public int? WorkingMonths { get; set; }

    public List<SelectListItem> Jobs { get; set; } = new();
    public List<SelectListItem> Sectors { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Session kontrolü
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var customerId))
        {
            _logger.LogWarning("Meslek bilgileri sayfası: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        // Meslek ve sektör listelerini yükle
        LoadJobs();
        LoadSectors();

        // Mevcut meslek bilgilerini çek
        try
        {
            var response = await _apiService.GetJobInfo(customerId);
            if (response.Success && response.Value != null)
            {
                var jobInfo = response.Value;
                WorkStatus = jobInfo.CustomerWork;
                JobId = jobInfo.JobGroupId;
                SectorId = jobInfo.JobGroupId; // Varsayılan olarak aynı ID kullanılıyor
                CompanyName = jobInfo.TitleCompany;
                Position = jobInfo.CompanyPosition;
                WorkingYears = jobInfo.WorkingYears;
                WorkingMonths = jobInfo.WorkingMonth;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Meslek bilgileri yükleme hatası");
            ErrorMessage = "Meslek bilgileri yüklenirken bir hata oluştu.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Session kontrolü
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var customerId))
        {
            _logger.LogWarning("Meslek bilgileri kaydetme: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        // Listeleri tekrar yükle
        LoadJobs();
        LoadSectors();

        // Conditional validation
        if (WorkStatus != 7) // "Çalışmıyorum" değilse
        {
            if (!JobId.HasValue)
            {
                ModelState.AddModelError("JobId", "Meslek seçimi gereklidir.");
            }
            if (!SectorId.HasValue)
            {
                ModelState.AddModelError("SectorId", "Sektör seçimi gereklidir.");
            }
            if (string.IsNullOrWhiteSpace(CompanyName))
            {
                ModelState.AddModelError("CompanyName", "Firma adı gereklidir.");
            }
            if (string.IsNullOrWhiteSpace(Position))
            {
                ModelState.AddModelError("Position", "Pozisyon gereklidir.");
            }
            if (!WorkingYears.HasValue)
            {
                ModelState.AddModelError("WorkingYears", "Çalışma yılı gereklidir.");
            }
            if (!WorkingMonths.HasValue)
            {
                ModelState.AddModelError("WorkingMonths", "Çalışma ayı gereklidir.");
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var request = new JobProfileRequest
            {
                CustomerId = customerId,
                JobId = JobId ?? 0,
                SectorId = SectorId ?? 0
            };

            var response = await _apiService.SaveJobProfile(request);

            if (response.Success)
            {
                _logger.LogInformation("Meslek bilgileri kaydedildi: CustomerId={CustomerId}", customerId);
                SuccessMessage = "Meslek bilgileriniz başarıyla kaydedildi.";
                return Page();
            }

            ErrorMessage = response.Message ?? "Meslek bilgileri kaydedilemedi.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Meslek bilgileri kaydetme hatası");
            ErrorMessage = "Bir hata oluştu. Lütfen tekrar deneyin.";
            return Page();
        }
    }

    private void LoadJobs()
    {
        Jobs = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Meslek Seçiniz", Selected = !JobId.HasValue }
        };

        try
        {
            var jsonPath = System.IO.Path.Combine(_environment.WebRootPath, "data", "job_id.json");
            if (System.IO.File.Exists(jsonPath))
            {
                var jsonContent = System.IO.File.ReadAllText(jsonPath);
                var jobs = JsonSerializer.Deserialize<List<JobItem>>(jsonContent);
                
                if (jobs != null)
                {
                    foreach (var job in jobs)
                    {
                        Jobs.Add(new SelectListItem
                        {
                            Value = job.id.ToString(),
                            Text = job.name,
                            Selected = JobId == job.id
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Meslek listesi yükleme hatası");
        }
    }

    private void LoadSectors()
    {
        Sectors = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Sektör Seçiniz", Selected = !SectorId.HasValue }
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
                            Selected = SectorId == sector.id
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

    private class JobItem
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
    }

    private class SectorItem
    {
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
    }
}
