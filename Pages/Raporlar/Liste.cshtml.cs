using InteraktifKredi.Models;
using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages.Raporlar;

public class ListeModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<ListeModel> _logger;

    public ListeModel(IApiService apiService, ILogger<ListeModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public List<ReportModel> Reports { get; set; } = new();
    public ReportDetailModel? LatestReportDetail { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsLoading { get; set; } = true;

    public async Task<IActionResult> OnGetAsync(long? id = null)
    {
        // Session kontrolü
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var customerId))
        {
            _logger.LogWarning("Rapor listesi sayfası: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        IsLoading = true;

        try
        {
            var response = await _apiService.GetReportList(customerId);
            if (response.Success && response.Value != null)
            {
                Reports = response.Value.OrderByDescending(r => r.ReportDate).ToList();
                _logger.LogInformation("Rapor listesi yüklendi: CustomerId={CustomerId}, Count={Count}", 
                    customerId, Reports.Count);

                // En güncel raporun detayını çek
                if (Reports.Any())
                {
                    var reportIdToLoad = id ?? Reports.First().ReportId;
                    var detailResponse = await _apiService.GetReportDetail(reportIdToLoad);
                    if (detailResponse.Success && detailResponse.Value != null)
                    {
                        LatestReportDetail = detailResponse.Value;
                        _logger.LogInformation("En güncel rapor detayı yüklendi: ReportId={ReportId}", reportIdToLoad);
                    }
                }
            }
            else
            {
                ErrorMessage = response.Message ?? "Rapor listesi yüklenemedi.";
                Reports = new List<ReportModel>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rapor listesi yükleme hatası");
            ErrorMessage = "Rapor listesi yüklenirken bir hata oluştu.";
            Reports = new List<ReportModel>();
        }
        finally
        {
            IsLoading = false;
        }

        return Page();
    }
}
