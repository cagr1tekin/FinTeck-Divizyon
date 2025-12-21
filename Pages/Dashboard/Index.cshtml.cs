using InteraktifKredi.Models;
using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IApiService apiService, ILogger<IndexModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    public string? CustomerName { get; set; }
    public int ProfileCompletionPercent { get; set; }
    public bool HasAddress { get; set; }
    public bool HasJobProfile { get; set; }
    public bool HasIncomeInfo { get; set; }
    public bool HasSpouseInfo { get; set; }
    public List<string> MissingFields { get; set; } = new();
    
    // Currency data
    public CurrencyResponseModel? UsdCurrencyData { get; set; }
    public CurrencyResponseModel? EurCurrencyData { get; set; }
    public CurrencyResponseModel? TryCurrencyData { get; set; }
    public CurrencyResponseModel? AudCurrencyData { get; set; }
    public CurrencyResponseModel? DkkCurrencyData { get; set; }
    public CurrencyResponseModel? CadCurrencyData { get; set; }
    public CurrencyResponseModel? PlnCurrencyData { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Session'dan CustomerId'yi al
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var customerId))
        {
            _logger.LogWarning("Dashboard: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        // Session'dan kullanıcı adını al
        var tckn = HttpContext.Session.GetString("TCKN");
        CustomerName = HttpContext.Session.GetString("CustomerName");

        // Eğer session'da yoksa, API'den çek (şimdilik session'dan al)
        if (string.IsNullOrEmpty(CustomerName) && !string.IsNullOrEmpty(tckn))
        {
            // TCKN'dan isim çıkarılamaz, bu yüzden session'dan alınacak
            // Veya API'den customer bilgilerini çekebiliriz
            CustomerName = "Müşteri"; // Varsayılan
        }

        // Profil tamamlama durumunu kontrol et
        await CheckProfileCompletion(customerId);

        // Currency verilerini çek (Server-side initial load)
        await LoadCurrencyData();

        return Page();
    }
    
    private async Task LoadCurrencyData()
    {
        try
        {
            // TRY base endpoint'inden tüm kurları tek seferde al
            // Bu endpoint AUDTRY, DKKTRY, CADTRY, PLNTRY dahil tüm kurları içerir
            var tryResponse = await _apiService.GetCurrencyRate("TRY");
            
            if (tryResponse.Success && tryResponse.Data != null)
            {
                // Tüm currency data'ları aynı response'dan al
                UsdCurrencyData = tryResponse.Data;
                EurCurrencyData = tryResponse.Data;
                TryCurrencyData = tryResponse.Data;
                AudCurrencyData = tryResponse.Data;
                DkkCurrencyData = tryResponse.Data;
                CadCurrencyData = tryResponse.Data;
                PlnCurrencyData = tryResponse.Data;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Currency data yüklenemedi (server-side)");
            // Hata durumunda sessizce devam et, client-side dener
        }
    }

    private async Task CheckProfileCompletion(long customerId)
    {
        try
        {
            // Adres bilgisi kontrolü
            var addressResponse = await _apiService.GetCustomerAddress(customerId);
            HasAddress = addressResponse.Success && addressResponse.Value != null && 
                        !string.IsNullOrEmpty(addressResponse.Value.AddressLine);

            // Meslek bilgisi kontrolü
            var jobResponse = await _apiService.GetJobInfo(customerId);
            HasJobProfile = jobResponse.Success && jobResponse.Value != null && 
                           jobResponse.Value.JobGroupId > 0;

            // Gelir bilgisi kontrolü (FinanceAssets)
            var financeResponse = await _apiService.GetFinanceAssets(customerId);
            HasIncomeInfo = financeResponse.Success && financeResponse.Value != null && 
                           financeResponse.Value.SalaryAmount > 0;

            // Eş bilgisi kontrolü
            var wifeResponse = await _apiService.GetWifeInfo(customerId);
            HasSpouseInfo = wifeResponse.Success && wifeResponse.Value != null;

            // Eksik alanları belirle
            if (!HasAddress) MissingFields.Add("Adres Bilgileri");
            if (!HasJobProfile) MissingFields.Add("Meslek Bilgileri");
            if (!HasIncomeInfo) MissingFields.Add("Gelir Bilgileri");
            if (!HasSpouseInfo) MissingFields.Add("Eş Bilgileri");

            // Tamamlama yüzdesini hesapla
            int completedFields = 0;
            int totalFields = 4;

            if (HasAddress) completedFields++;
            if (HasJobProfile) completedFields++;
            if (HasIncomeInfo) completedFields++;
            if (HasSpouseInfo) completedFields++;

            ProfileCompletionPercent = (int)Math.Round((double)completedFields / totalFields * 100);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Profil tamamlama kontrolü hatası: CustomerId={CustomerId}", customerId);
            ProfileCompletionPercent = 0;
        }
    }
}
