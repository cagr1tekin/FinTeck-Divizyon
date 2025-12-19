using InteraktifKredi.Models;
using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages.Raporlar;

public class DetayModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<DetayModel> _logger;

    public DetayModel(IApiService apiService, ILogger<DetayModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public long? Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public long? ReportId { get; set; }

    public ReportDetailModel? ReportDetail { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsLoading { get; set; } = true;

    public async Task<IActionResult> OnGetAsync()
    {
        // Session kontrolü
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr))
        {
            _logger.LogWarning("Rapor detay sayfası: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        // ReportId'yi al (Id veya ReportId parametresinden)
        var reportId = Id ?? ReportId;
        if (!reportId.HasValue || reportId.Value <= 0)
        {
            _logger.LogWarning("Rapor detay sayfası: Geçersiz ReportId");
            ErrorMessage = "Geçersiz rapor ID'si.";
            IsLoading = false;
            return Page();
        }

        IsLoading = true;

        try
        {
            var response = await _apiService.GetReportDetail(reportId.Value);
            if (response.Success && response.Value != null)
            {
                ReportDetail = response.Value;
                _logger.LogInformation("Rapor detayı yüklendi: ReportId={ReportId}", reportId);
            }
            else
            {
                ErrorMessage = response.Message ?? "Rapor detayı yüklenemedi.";
                ReportDetail = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rapor detayı yükleme hatası: ReportId={ReportId}", reportId);
            ErrorMessage = "Rapor detayı yüklenirken bir hata oluştu.";
            ReportDetail = null;
        }
        finally
        {
            IsLoading = false;
        }

        return Page();
    }
}
