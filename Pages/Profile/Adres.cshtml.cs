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
    public string OccupationName { get; set; } = string.Empty;
}

public class SectorItem
{
    public int Id { get; set; }
    public string JobGroupName { get; set; } = string.Empty;
}

public class JobListResponse
{
    public List<JobItem>? Value { get; set; }
}

public class SectorListResponse
{
    public List<SectorItem>? Value { get; set; }
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

    // API Response Models
    public AddressInfo? AddressData { get; set; }
    public JobProfileModel? JobData { get; set; }
    public FinanceModel? FinanceData { get; set; }
    public WifeInfoModel? WifeData { get; set; }

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
        await LoadCitiesAsync();
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
                AddressData = addressResponse.Value;
                if (addressResponse.Value.CityId.HasValue)
                {
                    CityId = addressResponse.Value.CityId.Value;
                    await LoadDistrictsAsync(CityId);
                    
                    if (addressResponse.Value.TownId.HasValue)
                    {
                        TownId = addressResponse.Value.TownId.Value;
                    }
                }
            }
            else
            {
                // API'den veri gelmediyse boş AddressInfo oluştur
                AddressData = new AddressInfo
                {
                    CustomerId = customerId,
                    CityId = null,
                    TownId = null,
                    Address = string.Empty
                };
            }

            // Meslek bilgileri
            var jobResponse = await _apiService.GetJobInfo(customerId);
            if (jobResponse.Success && jobResponse.Value != null)
            {
                JobData = jobResponse.Value;
            }

            // Finansal bilgiler
            var financeResponse = await _apiService.GetFinanceAssets(customerId);
            if (financeResponse.Success && financeResponse.Value != null)
            {
                FinanceData = financeResponse.Value;
            }

            // Eş bilgileri
            var wifeResponse = await _apiService.GetWifeInfo(customerId);
            if (wifeResponse.Success && wifeResponse.Value != null)
            {
                WifeData = wifeResponse.Value;
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
        await LoadCitiesAsync();
        if (CityId > 0)
        {
            await LoadDistrictsAsync(CityId);
        }
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
                CustomerWork = 5, // Varsayılan: çalışıyor (5 = özel sektör)
                JobGroupId = JobId,
                WorkingYears = WorkingYears,
                WorkingMonth = WorkingMonths,
                TitleCompany = CompanyName ?? string.Empty,
                CompanyPosition = Position ?? string.Empty
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

    // AJAX Handler'ları - Edit/Save işlemleri için
    public async Task<IActionResult> OnGetGetDistrictsAsync([FromQuery] int cityId)
    {
        try
        {
            await LoadDistrictsAsync(cityId);
            var districts = Districts.Select(d => new { value = d.Value, text = d.Text }).ToList();
            return new JsonResult(new { districts = districts });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İlçe listesi yükleme hatası");
            return new JsonResult(new { districts = new List<object>() });
        }
    }

    public async Task<IActionResult> OnPostSaveAddressAsync([FromBody] AddressModel addressModel)
    {
        try
        {
            // Session kontrolü
            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var sessionCustomerId))
            {
                _logger.LogWarning("Adres kaydetme: Session'da CustomerId yok");
                return new JsonResult(new { success = false, message = "Oturum süresi dolmuş. Lütfen tekrar giriş yapın." });
            }

            // Model validation
            if (addressModel == null)
            {
                _logger.LogWarning("Adres kaydetme: Model null");
                return new JsonResult(new { success = false, message = "Geçersiz veri gönderildi." });
            }

            // CustomerId'yi session'dan al (güvenlik için)
            addressModel.CustomerId = sessionCustomerId;

            // Validation
            if (addressModel.CityId <= 0)
            {
                return new JsonResult(new { success = false, message = "Lütfen bir il seçiniz." });
            }

            if (addressModel.TownId <= 0)
            {
                return new JsonResult(new { success = false, message = "Lütfen bir ilçe seçiniz." });
            }

            _logger.LogInformation("Adres kaydediliyor: CustomerId={CustomerId}, CityId={CityId}, TownId={TownId}, Address={Address}", 
                addressModel.CustomerId, addressModel.CityId, addressModel.TownId, addressModel.Address);

            var result = await _apiService.SaveAddress(addressModel);
            
            if (result.Success)
            {
                _logger.LogInformation("Adres kaydedildi: CustomerId={CustomerId}", addressModel.CustomerId);
                
                // İl ve ilçe isimlerini al ve JSON olarak session'a kaydet
                try
                {
                    string cityName = string.Empty;
                    string districtName = string.Empty;
                    
                    // İl ismini al
                    var provincesResponse = await _apiService.GetProvinces();
                    if (provincesResponse.Success && provincesResponse.Data != null)
                    {
                        var province = provincesResponse.Data.FirstOrDefault(p => p.Id == addressModel.CityId);
                        cityName = province?.Name ?? string.Empty;
                    }
                    
                    // İlçe ismini al
                    var districtsResponse = await _apiService.GetDistrictsByProvinceId(addressModel.CityId);
                    if (districtsResponse.Success && districtsResponse.Data != null)
                    {
                        var district = districtsResponse.Data.FirstOrDefault(d => d.Id == addressModel.TownId);
                        districtName = district?.Name ?? string.Empty;
                    }
                    
                    // Eğer API'den isim gelmediyse, mevcut listelerden al (fallback)
                    if (string.IsNullOrEmpty(cityName) || string.IsNullOrEmpty(districtName))
                    {
                        await LoadCitiesAsync();
                        await LoadDistrictsAsync(addressModel.CityId);
                        
                        if (string.IsNullOrEmpty(cityName))
                        {
                            cityName = Cities.FirstOrDefault(c => c.Value == addressModel.CityId.ToString())?.Text ?? string.Empty;
                        }
                        if (string.IsNullOrEmpty(districtName))
                        {
                            districtName = Districts.FirstOrDefault(d => d.Value == addressModel.TownId.ToString())?.Text ?? string.Empty;
                        }
                    }
                    
                    // JSON formatında session'a kaydet
                    var addressJson = new
                    {
                        CustomerId = addressModel.CustomerId,
                        CityId = addressModel.CityId,
                        CityName = cityName,
                        TownId = addressModel.TownId,
                        DistrictName = districtName,
                        Address = addressModel.Address ?? string.Empty,
                        PostalCode = addressModel.PostalCode
                    };
                    
                    var addressJsonString = JsonSerializer.Serialize(addressJson);
                    HttpContext.Session.SetString("CustomerAddress", addressJsonString);
                    
                    _logger.LogInformation("Adres bilgisi session'a kaydedildi (JSON): CustomerId={CustomerId}, City={CityName}, District={DistrictName}", 
                        addressModel.CustomerId, cityName, districtName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Adres bilgisi session'a kaydedilemedi, ancak API'ye kayıt başarılı: {Error}", ex.Message);
                }
            }
            else
            {
                _logger.LogWarning("Adres kaydedilemedi: CustomerId={CustomerId}, Message={Message}", 
                    addressModel.CustomerId, result.Message);
            }

            return new JsonResult(new { success = result.Success, message = result.Message ?? "Adres bilgileri güncellendi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Adres kaydetme hatası: CustomerId={CustomerId}", addressModel?.CustomerId ?? 0);
            return new JsonResult(new { success = false, message = "Bir hata oluştu: " + ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSaveJobAsync([FromBody] JobProfileRequest jobRequest)
    {
        try
        {
            var result = await _apiService.SaveJobProfile(jobRequest);
            return new JsonResult(new { success = result.Success, message = result.Message ?? "İş bilgileri güncellendi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İş bilgileri kaydetme hatası");
            return new JsonResult(new { success = false, message = "Bir hata oluştu" });
        }
    }

    public async Task<IActionResult> OnPostSaveWifeAsync([FromBody] WifeInfoModel wifeInfo)
    {
        try
        {
            // Session kontrolü
            var customerIdStr = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerIdStr) || !long.TryParse(customerIdStr, out var sessionCustomerId))
            {
                _logger.LogWarning("Eş bilgileri kaydetme: Session'da CustomerId yok");
                return new JsonResult(new { success = false, message = "Oturum süresi dolmuş. Lütfen tekrar giriş yapın." });
            }

            // Model validation
            if (wifeInfo == null)
            {
                _logger.LogWarning("Eş bilgileri kaydetme: Model null");
                return new JsonResult(new { success = false, message = "Geçersiz veri gönderildi." });
            }

            // CustomerId'yi session'dan al (güvenlik için)
            wifeInfo.CustomerId = sessionCustomerId;

            _logger.LogInformation("Eş bilgileri kaydediliyor: CustomerId={CustomerId}, MaritalStatus={MaritalStatus}, WorkWife={WorkWife}, WifeSalaryAmount={WifeSalaryAmount}", 
                wifeInfo.CustomerId, wifeInfo.MaritalStatus, wifeInfo.WorkWife, wifeInfo.WifeSalaryAmount);

            var result = await _apiService.SaveWifeInfo(wifeInfo);
            
            if (result.Success)
            {
                _logger.LogInformation("Eş bilgileri kaydedildi: CustomerId={CustomerId}", wifeInfo.CustomerId);
            }
            else
            {
                _logger.LogWarning("Eş bilgileri kaydedilemedi: CustomerId={CustomerId}, Message={Message}", 
                    wifeInfo.CustomerId, result.Message);
            }

            return new JsonResult(new { success = result.Success, message = result.Message ?? "Eş bilgileri güncellendi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eş bilgileri kaydetme hatası: CustomerId={CustomerId}", wifeInfo?.CustomerId ?? 0);
            return new JsonResult(new { success = false, message = "Bir hata oluştu: " + ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSaveFinanceAsync([FromBody] FinanceModel financeModel)
    {
        try
        {
            var result = await _apiService.SaveFinanceAssets(financeModel);
            return new JsonResult(new { success = result.Success, message = result.Message ?? "Finansal bilgiler güncellendi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Finansal bilgiler kaydetme hatası");
            return new JsonResult(new { success = false, message = "Bir hata oluştu" });
        }
    }

    private async Task LoadCitiesAsync()
    {
        Cities = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "İl Seçiniz", Selected = CityId == 0 }
        };

        try
        {
            var response = await _apiService.GetProvinces();
            if (response.Success && response.Data != null && response.Data.Any())
            {
                foreach (var province in response.Data)
                {
                    Cities.Add(new SelectListItem
                    {
                        Value = province.Id.ToString(),
                        Text = province.Name,
                        Selected = CityId == province.Id
                    });
                }
                _logger.LogInformation("İller TurkeyAPI'den yüklendi: Count={Count}", response.Data.Count);
                return;
            }
            else
            {
                _logger.LogWarning("İller TurkeyAPI'den alınamadı, fallback liste kullanılıyor");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İller yükleme hatası (TurkeyAPI), fallback liste kullanılıyor");
        }

        // Fallback: Hardcoded liste (API başarısız olursa)
        LoadCitiesFallback();
    }

    private void LoadCitiesFallback()
    {
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

    private async Task LoadDistrictsAsync(int cityId)
    {
        Districts = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "İlçe Seçiniz", Selected = TownId == 0 }
        };

        if (cityId <= 0) return;

        try
        {
            var response = await _apiService.GetDistrictsByProvinceId(cityId);
            if (response.Success && response.Data != null && response.Data.Any())
            {
                foreach (var district in response.Data)
                {
                    Districts.Add(new SelectListItem
                    {
                        Value = district.Id.ToString(),
                        Text = district.Name,
                        Selected = TownId == district.Id
                    });
                }
                _logger.LogInformation("İlçeler TurkeyAPI'den yüklendi: CityId={CityId}, Count={Count}", cityId, response.Data.Count);
                return;
            }
            else
            {
                _logger.LogWarning("İlçeler TurkeyAPI'den alınamadı: CityId={CityId}", cityId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İlçeler yükleme hatası (TurkeyAPI): CityId={CityId}", cityId);
        }

        // Fallback: Hardcoded liste (API başarısız olursa)
        LoadDistrictsFallback(cityId);
    }

    private void LoadDistrictsFallback(int cityId)
    {
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
            // JSON dosyası root dizinde
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "job_id.json");
            if (!System.IO.File.Exists(jsonPath))
            {
                // Alternatif olarak wwwroot/data klasöründe de bak
                jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "job_id.json");
            }
            
            if (System.IO.File.Exists(jsonPath))
            {
                var jsonContent = System.IO.File.ReadAllText(jsonPath);
                var response = JsonSerializer.Deserialize<JobListResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (response?.Value != null)
                {
                    foreach (var job in response.Value)
                    {
                        Jobs.Add(new SelectListItem
                        {
                            Value = job.Id.ToString(),
                            Text = job.OccupationName,
                            Selected = JobId == job.Id
                        });
                    }
                }
            }
            else
            {
                _logger.LogWarning("job_id.json dosyası bulunamadı: {Path}", jsonPath);
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
            new SelectListItem { Value = "", Text = "Meslek Grubu Seçiniz", Selected = SectorId == 0 }
        };

        try
        {
            // JSON dosyası root dizinde
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "sektor_id.json");
            if (!System.IO.File.Exists(jsonPath))
            {
                // Alternatif olarak wwwroot/data klasöründe de bak
                jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "sektor_id.json");
            }
            
            if (System.IO.File.Exists(jsonPath))
            {
                var jsonContent = System.IO.File.ReadAllText(jsonPath);
                var response = JsonSerializer.Deserialize<SectorListResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (response?.Value != null)
                {
                    foreach (var sector in response.Value)
                    {
                        Sectors.Add(new SelectListItem
                        {
                            Value = sector.Id.ToString(),
                            Text = sector.JobGroupName,
                            Selected = SectorId == sector.Id
                        });
                    }
                }
            }
            else
            {
                _logger.LogWarning("sektor_id.json dosyası bulunamadı: {Path}", jsonPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Meslek grubu listesi yüklenemedi, varsayılan liste kullanılıyor");
            // Fallback: Basit liste
            var defaultSectors = new[] { "Kamu Çalışanıyım", "Özel Sektör Çalışanıyım", "İşyerim var /Ortağım", "Emekliyim", "Diğer" };
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
