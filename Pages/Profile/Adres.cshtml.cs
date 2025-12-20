using InteraktifKredi.Models;
using InteraktifKredi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace InteraktifKredi.Pages.Profile;

// JSON deserialization için helper class'lar
public class JobItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SectorItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AdresModel : PageModel
{
    private readonly IApiService _apiService;
    private readonly ILogger<AdresModel> _logger;

    public AdresModel(IApiService apiService, ILogger<AdresModel> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    // Kişisel Bilgiler
    [BindProperty]
    public string? FirstName { get; set; }

    [BindProperty]
    public string? LastName { get; set; }

    [BindProperty]
    public string? TCKN { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Telefon numarası gereklidir.")]
    [RegularExpression(@"^5\d{9}$", ErrorMessage = "Geçerli bir telefon numarası giriniz (5XXXXXXXXX).")]
    public string Phone { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "İl seçimi gereklidir.")]
    [Range(1, 81, ErrorMessage = "Geçerli bir il seçiniz.")]
    public int CityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "İlçe seçimi gereklidir.")]
    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir ilçe seçiniz.")]
    public int TownId { get; set; }

    // İş ve Gelir Bilgileri
    [BindProperty]
    [Required(ErrorMessage = "Meslek seçimi gereklidir.")]
    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir meslek seçiniz.")]
    public int JobId { get; set; }

    [BindProperty]
    [StringLength(200, ErrorMessage = "Şirket adı en fazla 200 karakter olabilir.")]
    public string? CompanyName { get; set; }

    [BindProperty]
    [StringLength(100, ErrorMessage = "Pozisyon en fazla 100 karakter olabilir.")]
    public string? Position { get; set; }

    [BindProperty]
    [Range(0, 50, ErrorMessage = "Çalışma yılı 0-50 arasında olmalıdır.")]
    public int WorkingYears { get; set; }

    [BindProperty]
    [Range(0, 11, ErrorMessage = "Çalışma ayı 0-11 arasında olmalıdır.")]
    public int WorkingMonths { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Aylık gelir gereklidir.")]
    [Range(0.01, 999999.99, ErrorMessage = "Aylık gelir 0,01 ile 999.999,99 arasında olmalıdır.")]
    public decimal MonthlyIncome { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Sektör seçimi gereklidir.")]
    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir sektör seçiniz.")]
    public int SectorId { get; set; }

    // Ek Bilgiler
    [BindProperty]
    [Required(ErrorMessage = "Medeni durum seçimi gereklidir.")]
    public string? MaritalStatus { get; set; }

    // Eş Bilgileri (Conditional)
    [BindProperty]
    public bool WorkSpouse { get; set; }

    [BindProperty]
    [Range(0.01, 999999.99, ErrorMessage = "Eş maaşı 0,01 ile 999.999,99 arasında olmalıdır.")]
    public decimal? SpouseSalary { get; set; }

    // Select Lists
    public List<SelectListItem> Cities { get; set; } = new();
    public List<SelectListItem> Districts { get; set; } = new();
    public List<SelectListItem> Jobs { get; set; } = new();
    public List<SelectListItem> Sectors { get; set; } = new();
    public List<SelectListItem> MaritalStatuses { get; set; } = new();

    // Display Properties
    public string? CustomerName { get; set; }
    public string? CustomerTitle { get; set; }
    public int CreditScore { get; set; } = 1450; // Örnek kredi notu
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Session kontrolü
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var customerId))
        {
            _logger.LogWarning("Profil sayfası: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        // Session'dan kullanıcı bilgilerini al
        CustomerName = HttpContext.Session.GetString("CustomerName");
        TCKN = HttpContext.Session.GetString("TCKN");
        Phone = HttpContext.Session.GetString("GSM") ?? string.Empty;
        CustomerTitle = "Kullanıcı"; // Varsayılan ünvan, API'den çekilebilir
        
        // İsim ve soyisim ayır (varsa)
        if (!string.IsNullOrEmpty(CustomerName))
        {
            var nameParts = CustomerName.Split(' ');
            FirstName = nameParts.Length > 0 ? nameParts[0] : CustomerName;
            LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : string.Empty;
        }

        // Select listelerini doldur
        LoadCities();
        LoadJobs();
        LoadSectors();
        LoadMaritalStatuses();

        // Mevcut bilgileri API'den çek
        try
        {
            // Adres bilgileri
            var addressResponse = await _apiService.GetCustomerAddress(customerId);
            if (addressResponse.Success && addressResponse.Value != null)
            {
                if (!string.IsNullOrEmpty(addressResponse.Value.City))
                {
                    var city = Cities.FirstOrDefault(c => c.Text == addressResponse.Value.City);
                    if (city != null)
                    {
                        CityId = int.Parse(city.Value);
                        LoadDistricts(CityId);
                        
                        if (!string.IsNullOrEmpty(addressResponse.Value.District))
                        {
                            var district = Districts.FirstOrDefault(d => d.Text == addressResponse.Value.District);
                            if (district != null)
                            {
                                TownId = int.Parse(district.Value);
                            }
                        }
                    }
                }
            }

            // Meslek bilgileri
            var jobResponse = await _apiService.GetJobInfo(customerId);
            if (jobResponse.Success && jobResponse.Value != null)
            {
                JobId = jobResponse.Value.JobGroupId;
                CompanyName = jobResponse.Value.TitleCompany;
                Position = jobResponse.Value.CompanyPosition;
                WorkingYears = jobResponse.Value.WorkingYears;
                WorkingMonths = jobResponse.Value.WorkingMonth;
            }
            else
            {
                // Varsayılan değerler
                JobId = 0;
                WorkingYears = 0;
                WorkingMonths = 0;
            }

            // Finansal bilgiler
            var financeResponse = await _apiService.GetFinanceAssets(customerId);
            if (financeResponse.Success && financeResponse.Value != null)
            {
                MonthlyIncome = financeResponse.Value.SalaryAmount;
                SectorId = financeResponse.Value.WorkSector;
            }
            else
            {
                // Varsayılan değerler
                MonthlyIncome = 0;
                SectorId = 0;
            }

            // Eş bilgileri
            var wifeResponse = await _apiService.GetWifeInfo(customerId);
            if (wifeResponse.Success && wifeResponse.Value != null)
            {
                MaritalStatus = wifeResponse.Value.MaritalStatus ? "Evli" : "Bekar";
                WorkSpouse = wifeResponse.Value.WorkWife;
                SpouseSalary = wifeResponse.Value.WifeSalaryAmount > 0 ? wifeResponse.Value.WifeSalaryAmount : null;
            }
            else
            {
                // Varsayılan değerler
                MaritalStatus = "Bekar";
                WorkSpouse = false;
                SpouseSalary = null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Profil bilgileri yükleme hatası");
            ErrorMessage = "Profil bilgileri yüklenirken bir hata oluştu.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Session kontrolü
        var customerIdStr = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var customerId))
        {
            _logger.LogWarning("Profil kaydetme: Session'da CustomerId yok");
            return RedirectToPage("/Onboarding/TcknGsm");
        }

        // Select listelerini doldur
        LoadCities();
        LoadDistricts(CityId);
        LoadJobs();
        LoadSectors();
        LoadMaritalStatuses();

        // Session'dan kullanıcı bilgilerini al
        CustomerName = HttpContext.Session.GetString("CustomerName");
        TCKN = HttpContext.Session.GetString("TCKN");

        // Para değerleri hidden input'lardan gelecek (Model binding otomatik yapacak)

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // 1. Adres bilgilerini kaydet
            var addressModel = new AddressModel
            {
                CustomerId = customerId,
                CityId = CityId,
                TownId = TownId,
                Address = string.Empty, // Adres satırı şimdilik boş
                PostalCode = null
            };
            await _apiService.SaveAddress(addressModel);

            // 2. Meslek bilgilerini kaydet
            var jobRequest = new JobProfileRequest
            {
                CustomerId = customerId,
                JobId = JobId,
                SectorId = SectorId
            };
            await _apiService.SaveJobProfile(jobRequest);

            // 3. Finansal bilgileri kaydet
            var financeModel = new FinanceModel
            {
                CustomerId = customerId,
                WorkSector = SectorId,
                SalaryAmount = MonthlyIncome,
                CarStatus = false,
                HouseStatus = false
            };
            await _apiService.SaveFinanceAssets(financeModel);

            // 4. Eş bilgilerini kaydet (eğer evli ise)
            if (MaritalStatus == "Evli")
            {
                var wifeModel = new WifeInfoModel
                {
                    CustomerId = customerId,
                    MaritalStatus = true,
                    WorkWife = WorkSpouse,
                    WifeSalaryAmount = SpouseSalary ?? 0
                };
                await _apiService.SaveWifeInfo(wifeModel);
            }

            _logger.LogInformation("Profil bilgileri kaydedildi: CustomerId={CustomerId}", customerId);
            SuccessMessage = "Profil bilgileriniz başarıyla güncellendi.";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Profil bilgisi kaydetme hatası");
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
                    Value = (cityId * 100 + i + 1).ToString(),
                    Text = districts[i],
                    Selected = TownId == (cityId * 100 + i + 1)
                });
            }
        }
    }

    private void LoadJobs()
    {
        Jobs = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Meslek Seçiniz", Selected = JobId == 0 }
        };

        try
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "job_id.json");
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
                            Value = job.Id.ToString(),
                            Text = job.Name,
                            Selected = JobId == job.Id
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Meslek listesi yüklenemedi, varsayılan liste kullanılıyor");
            // Fallback: Basit liste
            var defaultJobs = new[] { "Mühendis", "Doktor", "Öğretmen", "Avukat", "Muhasebeci", "Diğer" };
            for (int i = 0; i < defaultJobs.Length; i++)
            {
                Jobs.Add(new SelectListItem
                {
                    Value = (i + 1).ToString(),
                    Text = defaultJobs[i],
                    Selected = JobId == (i + 1)
                });
            }
        }
    }

    private void LoadSectors()
    {
        Sectors = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Sektör Seçiniz", Selected = SectorId == 0 }
        };

        try
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "sektor_id.json");
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
                            Value = sector.Id.ToString(),
                            Text = sector.Name,
                            Selected = SectorId == sector.Id
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sektör listesi yüklenemedi, varsayılan liste kullanılıyor");
            // Fallback: Basit liste
            var defaultSectors = new[] { "Kamu", "Özel Sektör", "Sağlık", "Eğitim", "Finans", "Diğer" };
            for (int i = 0; i < defaultSectors.Length; i++)
            {
                Sectors.Add(new SelectListItem
                {
                    Value = (i + 1).ToString(),
                    Text = defaultSectors[i],
                    Selected = SectorId == (i + 1)
                });
            }
        }
    }

    private void LoadMaritalStatuses()
    {
        MaritalStatuses = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Seçiniz", Selected = string.IsNullOrEmpty(MaritalStatus) },
            new SelectListItem { Value = "Bekar", Text = "Bekar", Selected = MaritalStatus == "Bekar" },
            new SelectListItem { Value = "Evli", Text = "Evli", Selected = MaritalStatus == "Evli" },
            new SelectListItem { Value = "Boşanmış", Text = "Boşanmış", Selected = MaritalStatus == "Boşanmış" },
            new SelectListItem { Value = "Dul", Text = "Dul", Selected = MaritalStatus == "Dul" }
        };
    }
}
