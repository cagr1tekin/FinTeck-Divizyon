using InteraktifKredi.Models;
using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace InteraktifKredi.Pages.Profile;

public class AdresModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<AdresModel> _logger;

    public AdresModel(IApiService apiService, ILogger<AdresModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [BindProperty]
    [Required(ErrorMessage = "İl seçimi gereklidir.")]
    [Range(1, 81, ErrorMessage = "Geçerli bir il seçiniz.")]
    public int CityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "İlçe seçimi gereklidir.")]
    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir ilçe seçiniz.")]
    public int TownId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Açık adres gereklidir.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Açık adres 10-500 karakter arasında olmalıdır.")]
    public string Address { get; set; } = string.Empty;

    [BindProperty]
    [StringLength(5, MinimumLength = 5, ErrorMessage = "Posta kodu 5 haneli olmalıdır.")]
    [RegularExpression(@"^\d+$", ErrorMessage = "Posta kodu sadece rakam içermelidir.")]
    public string? PostalCode { get; set; }

    public List<SelectListItem> Cities { get; set; } = new();
    public List<SelectListItem> Districts { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Session kontrolü
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var customerId))
        {
            _logger.LogWarning("Adres sayfası: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        // İl listesini doldur
        LoadCities();

        // Mevcut adresi çek
        try
        {
            var response = await _apiService.GetCustomerAddress(customerId);
            if (response.Success && response.Value != null)
            {
                // Eğer API'den CityId ve TownId geliyorsa
                // Şimdilik sadece AddressLine, City, District string olarak gelebilir
                Address = response.Value.AddressLine ?? string.Empty;
                PostalCode = response.Value.PostalCode;

                // City ve District string ise, ID'leri bul
                if (!string.IsNullOrEmpty(response.Value.City))
                {
                    var city = Cities.FirstOrDefault(c => c.Text == response.Value.City);
                    if (city != null)
                    {
                        CityId = int.Parse(city.Value);
                        // İlçeleri yükle
                        LoadDistricts(CityId);
                        
                        if (!string.IsNullOrEmpty(response.Value.District))
                        {
                            var district = Districts.FirstOrDefault(d => d.Text == response.Value.District);
                            if (district != null)
                            {
                                TownId = int.Parse(district.Value);
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Adres bilgisi yükleme hatası");
            ErrorMessage = "Adres bilgisi yüklenirken bir hata oluştu.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Session kontrolü
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var customerId))
        {
            _logger.LogWarning("Adres kaydetme: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        // İl listesini doldur
        LoadCities();
        LoadDistricts(CityId);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var addressModel = new AddressModel
            {
                CustomerId = customerId,
                CityId = CityId,
                TownId = TownId,
                Address = Address,
                PostalCode = PostalCode
            };

            var response = await _apiService.SaveAddress(addressModel);

            if (response.Success)
            {
                _logger.LogInformation("Adres bilgisi kaydedildi: CustomerId={CustomerId}", customerId);
                SuccessMessage = "Adres bilgileriniz başarıyla kaydedildi.";
                return Page();
            }

            ErrorMessage = response.Message ?? "Adres bilgisi kaydedilemedi.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Adres bilgisi kaydetme hatası");
            ErrorMessage = "Bir hata oluştu. Lütfen tekrar deneyin.";
            return Page();
        }
    }

    private void LoadCities()
    {
        Cities = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "İl Seçiniz", Selected = CityId == 0 }
        };

        // Türkiye'nin 81 ili
        var cities = new[]
        {
            "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Amasya", "Ankara", "Antalya", "Artvin",
            "Aydın", "Balıkesir", "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur", "Bursa",
            "Çanakkale", "Çankırı", "Çorum", "Denizli", "Diyarbakır", "Edirne", "Elazığ", "Erzincan",
            "Erzurum", "Eskişehir", "Gaziantep", "Giresun", "Gümüşhane", "Hakkari", "Hatay", "Isparta",
            "Mersin", "İstanbul", "İzmir", "Kars", "Kastamonu", "Kayseri", "Kırklareli", "Kırşehir",
            "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa", "Kahramanmaraş", "Mardin", "Muğla",
            "Muş", "Nevşehir", "Niğde", "Ordu", "Rize", "Sakarya", "Samsun", "Siirt",
            "Sinop", "Sivas", "Tekirdağ", "Tokat", "Trabzon", "Tunceli", "Şanlıurfa", "Uşak",
            "Van", "Yozgat", "Zonguldak", "Aksaray", "Bayburt", "Karaman", "Kırıkkale", "Batman",
            "Şırnak", "Bartın", "Ardahan", "Iğdır", "Yalova", "Karabük", "Kilis", "Osmaniye", "Düzce"
        };

        for (int i = 0; i < cities.Length; i++)
        {
            Cities.Add(new SelectListItem
            {
                Value = (i + 1).ToString(),
                Text = cities[i],
                Selected = CityId == (i + 1)
            });
        }
    }

    private void LoadDistricts(int cityId)
    {
        Districts = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "İlçe Seçiniz", Selected = TownId == 0 }
        };

        // Örnek ilçe verileri - Gerçek uygulamada API'den çekilecek
        // Şimdilik sadece bazı iller için örnek veri
        var districtMap = new Dictionary<int, string[]>
        {
            { 34, new[] { "Adalar", "Bakırköy", "Beşiktaş", "Beykoz", "Beyoğlu", "Çatalca", "Eyüp", "Fatih", "Gaziosmanpaşa", "Kadıköy", "Kartal", "Sarıyer", "Silivri", "Şile", "Şişli", "Üsküdar", "Zeytinburnu" } },
            { 6, new[] { "Altındağ", "Ayaş", "Bala", "Beypazarı", "Çamlıdere", "Çankaya", "Çubuk", "Elmadağ", "Güdül", "Haymana", "Kalecik", "Kızılcahamam", "Nallıhan", "Polatlı", "Şereflikoçhisar", "Yenimahalle" } },
            { 35, new[] { "Aliağa", "Bayındır", "Bergama", "Bornova", "Çeşme", "Dikili", "Foça", "Karaburun", "Karşıyaka", "Kemalpaşa", "Kınık", "Kiraz", "Menemen", "Ödemiş", "Seferihisar", "Selçuk", "Tire", "Torbalı", "Urla" } }
        };

        if (districtMap.ContainsKey(cityId))
        {
            var districts = districtMap[cityId];
            for (int i = 0; i < districts.Length; i++)
            {
                Districts.Add(new SelectListItem
                {
                    Value = (cityId * 100 + i + 1).ToString(), // Basit ID oluşturma
                    Text = districts[i],
                    Selected = TownId == (cityId * 100 + i + 1)
                });
            }
        }
    }
}
