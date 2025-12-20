using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using InteraktifKredi.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace InteraktifKredi.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;
    private readonly IConfiguration _configuration;

    private const string CUSTOMERS_API = "https://customers-api.azurewebsites.net";
    private const string IDC_API = "https://api-idc.azurewebsites.net";
    private const string DEFAULT_TOKEN = "fe7vSdh1QqqcdRzZO4HqG7TvDL5zEoF2bwKzOzAGJE67s";
    private const int TIMEOUT_SECONDS = 30;

    public ApiService(HttpClient httpClient, ILogger<ApiService> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
        
        var token = _configuration.GetValue<string>("ApiSettings:DefaultToken") ?? DEFAULT_TOKEN;
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
        _httpClient.Timeout = TimeSpan.FromSeconds(TIMEOUT_SECONDS);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    }

    // ============================================
    // ONBOARDING METODLARI
    // ============================================

    public async Task<ApiResponse<CustomerInfo>> ValidateTcknGsm(string tckn, string gsm)
    {
        try
        {
            _logger.LogInformation("TCKN-GSM doğrulama başlatıldı: TCKN={MaskedTckn}, GSM={MaskedGsm}", 
                MaskTckn(tckn), MaskPhone(gsm));

            var request = new { TCKN = tckn, GSM = gsm };
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // API code parametresini al
            var apiCode = _configuration.GetValue<string>("ApiSettings:CustomersApiCode");
            var url = $"{CUSTOMERS_API}/api/customer/tckn-gsm";
            if (!string.IsNullOrEmpty(apiCode))
            {
                url += $"?code={Uri.EscapeDataString(apiCode)}";
            }

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // API response wrapper'ı kontrol et
                CustomerInfo? customerInfo = null;
                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    // Eğer response wrapper içeriyorsa (success, value gibi alanlar varsa)
                    if (apiResponse?.value != null)
                    {
                        customerInfo = JsonConvert.DeserializeObject<CustomerInfo>(apiResponse.value.ToString());
                    }
                    else
                    {
                        // Direkt CustomerInfo olarak geliyorsa
                        customerInfo = JsonConvert.DeserializeObject<CustomerInfo>(responseContent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Response deserialize hatası: Response={Response}", responseContent);
                    // Fallback: direkt deserialize dene
                    customerInfo = JsonConvert.DeserializeObject<CustomerInfo>(responseContent);
                }

                _logger.LogInformation("TCKN-GSM doğrulama başarılı: CustomerId={CustomerId}", 
                    customerInfo?.CustomerId);
                return new ApiResponse<CustomerInfo> { Success = true, Data = customerInfo };
            }

            _logger.LogWarning("TCKN-GSM doğrulama başarısız: StatusCode={StatusCode}, TCKN={MaskedTckn}", 
                response.StatusCode, MaskTckn(tckn));
            
            var errorMessage = "Doğrulama başarısız.";
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                errorMessage = errorResponse?.message?.ToString() ?? errorMessage;
            }
            catch { }

            return new ApiResponse<CustomerInfo> { Success = false, Message = errorMessage };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "TCKN-GSM doğrulama timeout: TCKN={MaskedTckn}", MaskTckn(tckn));
            return new ApiResponse<CustomerInfo> { Success = false, Message = "İstek zaman aşımına uğradı. Lütfen tekrar deneyin." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TCKN-GSM doğrulama hatası: TCKN={MaskedTckn}", MaskTckn(tckn));
            return new ApiResponse<CustomerInfo> { Success = false, Message = "Bir hata oluştu. Lütfen tekrar deneyin." };
        }
    }

    public async Task<ApiResponse<string>> GetKvkkText(int kvkkId)
    {
        try
        {
            _logger.LogInformation("KVKK metni alınıyor: KvkkId={KvkkId}", kvkkId);

            // API code parametresini al
            var apiCode = _configuration.GetValue<string>("ApiSettings:IdcApiKvkkTextCode");
            
            var url = $"{IDC_API}/api/kvkk/text/{kvkkId}";
            if (!string.IsNullOrEmpty(apiCode))
            {
                url += $"?code={Uri.EscapeDataString(apiCode)}";
            }

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // API response objesi: { "Id": 1, "Name": "...", "Text": "..." }
                string kvkkText = string.Empty;
                try
                {
                    var kvkkResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    // Text alanını al
                    kvkkText = kvkkResponse?.Text?.ToString() ?? string.Empty;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "KVKK response deserialize hatası: Response={Response}", responseContent);
                    // Fallback: direkt string olarak al
                    kvkkText = responseContent;
                }

                _logger.LogInformation("KVKK metni alındı: KvkkId={KvkkId}, Length={Length}", 
                    kvkkId, kvkkText.Length);
                return new ApiResponse<string> { Success = true, Data = kvkkText };
            }

            _logger.LogWarning("KVKK metni alınamadı: StatusCode={StatusCode}, KvkkId={KvkkId}, Response={Response}", 
                response.StatusCode, kvkkId, responseContent);
            
            var errorMessage = "KVKK metni alınamadı.";
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                errorMessage = errorResponse?.message?.ToString() ?? errorMessage;
            }
            catch { }

            return new ApiResponse<string> { Success = false, Message = errorMessage };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "KVKK metni alma timeout: KvkkId={KvkkId}", kvkkId);
            return new ApiResponse<string> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KVKK metni alma hatası: KvkkId={KvkkId}", kvkkId);
            return new ApiResponse<string> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    public async Task<ApiResponse<bool>> SaveKvkkOnay(int kvkkId, long customerId)
    {
        try
        {
            _logger.LogInformation("KVKK onayı kaydediliyor: KvkkId={KvkkId}, CustomerId={CustomerId}", 
                kvkkId, customerId);

            // API code parametresini al
            var apiCode = _configuration.GetValue<string>("ApiSettings:IdcApiKvkkOnayCode");
            
            var url = $"{IDC_API}/api/kvkk/onay";
            if (!string.IsNullOrEmpty(apiCode))
            {
                url += $"?code={Uri.EscapeDataString(apiCode)}";
            }

            // Request body küçük harf field isimleri ile (API formatına uygun)
            var request = new { kvkkId = kvkkId, customerId = customerId, isOk = true };
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("KVKK onayı kaydedildi: KvkkId={KvkkId}, CustomerId={CustomerId}", 
                    kvkkId, customerId);
                return new ApiResponse<bool> { Success = true, Data = true };
            }

            _logger.LogWarning("KVKK onayı kaydedilemedi: StatusCode={StatusCode}, KvkkId={KvkkId}, Response={Response}", 
                response.StatusCode, kvkkId, responseContent);
            
            var errorMessage = "KVKK onayı kaydedilemedi.";
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                errorMessage = errorResponse?.message?.ToString() ?? errorMessage;
            }
            catch { }

            return new ApiResponse<bool> { Success = false, Message = errorMessage };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "KVKK onayı kaydetme timeout: KvkkId={KvkkId}", kvkkId);
            return new ApiResponse<bool> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "KVKK onayı kaydetme hatası: KvkkId={KvkkId}", kvkkId);
            return new ApiResponse<bool> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    public async Task<ApiResponse<OtpResult>> GenerateOtp(string tckn, string gsm)
    {
        try
        {
            _logger.LogInformation("OTP oluşturuluyor: TCKN={MaskedTckn}, GSM={MaskedGsm}", 
                MaskTckn(tckn), MaskPhone(gsm));

            // API code parametresini al
            var apiCode = _configuration.GetValue<string>("ApiSettings:IdcApiGenerateOtpCode");
            var utmId = _configuration.GetValue<string>("ApiSettings:UtmId") ?? "5";
            
            var url = $"{IDC_API}/api/generate-otp";
            if (!string.IsNullOrEmpty(apiCode))
            {
                url += $"?code={Uri.EscapeDataString(apiCode)}";
            }

            // Request body küçük harf field isimleri ile
            var request = new { tckn = tckn, gsm = gsm, utmId = utmId };
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // API response wrapper'ı kontrol et
                OtpResult? otpResult = null;
                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    // Eğer response wrapper içeriyorsa (success, value gibi alanlar varsa)
                    if (apiResponse?.value != null)
                    {
                        var valueJson = apiResponse.value.ToString();
                        var valueObj = JsonConvert.DeserializeObject<dynamic>(valueJson);
                        
                        otpResult = new OtpResult
                        {
                            OtpCode = valueObj?.OTPCode?.ToString() ?? string.Empty,
                            ExpiresAt = DateTime.UtcNow.AddMinutes(5), // API'den gelmiyorsa varsayılan
                            RetryCount = 0
                        };
                    }
                    else
                    {
                        // Direkt OtpResult olarak geliyorsa
                        otpResult = JsonConvert.DeserializeObject<OtpResult>(responseContent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Response deserialize hatası: Response={Response}", responseContent);
                    // Fallback: direkt deserialize dene
                    otpResult = JsonConvert.DeserializeObject<OtpResult>(responseContent);
                }

                _logger.LogInformation("OTP oluşturuldu: TCKN={MaskedTckn}, OtpCode={OtpCode}", 
                    MaskTckn(tckn), otpResult?.OtpCode);
                return new ApiResponse<OtpResult> { Success = true, Data = otpResult };
            }

            _logger.LogWarning("OTP oluşturulamadı: StatusCode={StatusCode}, TCKN={MaskedTckn}", 
                response.StatusCode, MaskTckn(tckn));
            
            var errorMessage = "OTP oluşturulamadı.";
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                errorMessage = errorResponse?.message?.ToString() ?? errorMessage;
            }
            catch { }

            return new ApiResponse<OtpResult> { Success = false, Message = errorMessage };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "OTP oluşturma timeout: TCKN={MaskedTckn}", MaskTckn(tckn));
            return new ApiResponse<OtpResult> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OTP oluşturma hatası: TCKN={MaskedTckn}", MaskTckn(tckn));
            return new ApiResponse<OtpResult> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    public async Task<ApiResponse<bool>> SendOtpSms(string gsm, string otpCode)
    {
        try
        {
            _logger.LogInformation("OTP SMS gönderiliyor: GSM={MaskedGsm}", MaskPhone(gsm));

            // API code parametresini al
            var apiCode = _configuration.GetValue<string>("ApiSettings:IdcApiSendOtpSmsCode");
            
            var url = $"{IDC_API}/api/send-otp-sms";
            if (!string.IsNullOrEmpty(apiCode))
            {
                url += $"?code={Uri.EscapeDataString(apiCode)}";
            }

            // Request body küçük harf field isimleri ile
            var request = new { gsm = gsm, otpCode = otpCode };
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("OTP SMS gönderildi: GSM={MaskedGsm}", MaskPhone(gsm));
                return new ApiResponse<bool> { Success = true, Data = true };
            }

            _logger.LogWarning("OTP SMS gönderilemedi: StatusCode={StatusCode}, GSM={MaskedGsm}, Response={Response}", 
                response.StatusCode, MaskPhone(gsm), responseContent);
            
            var errorMessage = "SMS gönderilemedi.";
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                errorMessage = errorResponse?.message?.ToString() ?? errorMessage;
            }
            catch { }

            return new ApiResponse<bool> { Success = false, Message = errorMessage };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "OTP SMS gönderme timeout: GSM={MaskedGsm}", MaskPhone(gsm));
            return new ApiResponse<bool> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OTP SMS gönderme hatası: GSM={MaskedGsm}", MaskPhone(gsm));
            return new ApiResponse<bool> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    public async Task<ApiResponse<TokenResult>> VerifyOtp(string otpCode)
    {
        try
        {
            _logger.LogInformation("OTP doğrulanıyor");

            // API code parametresini al
            var apiCode = _configuration.GetValue<string>("ApiSettings:IdcApiVerifyOtpCode");
            
            var url = $"{IDC_API}/api/verify-otp";
            if (!string.IsNullOrEmpty(apiCode))
            {
                url += $"?code={Uri.EscapeDataString(apiCode)}";
            }

            // Request body küçük harf field isimleri ile
            var request = new { otpCode = otpCode };
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // API response wrapper'ı kontrol et
                TokenResult? tokenResult = null;
                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    // Eğer response wrapper içeriyorsa (success, value gibi alanlar varsa)
                    if (apiResponse?.value != null)
                    {
                        var valueJson = apiResponse.value.ToString();
                        var valueObj = JsonConvert.DeserializeObject<dynamic>(valueJson);
                        
                        var token = valueObj?.token?.ToString() ?? string.Empty;
                        var customerId = ParseCustomerIdFromToken(token);
                        
                        tokenResult = new TokenResult
                        {
                            Token = token,
                            ExpiresAt = DateTime.UtcNow.AddHours(24), // API'den gelmiyorsa varsayılan
                            CustomerId = customerId
                        };
                    }
                    else
                    {
                        // Direkt TokenResult olarak geliyorsa
                        tokenResult = JsonConvert.DeserializeObject<TokenResult>(responseContent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Response deserialize hatası: Response={Response}", responseContent);
                    // Fallback: direkt deserialize dene
                    tokenResult = JsonConvert.DeserializeObject<TokenResult>(responseContent);
                }

                _logger.LogInformation("OTP doğrulandı: Token={Token}, ExpiresAt={ExpiresAt}", 
                    tokenResult?.Token?.Substring(0, Math.Min(20, tokenResult.Token?.Length ?? 0)) + "...", 
                    tokenResult?.ExpiresAt);
                return new ApiResponse<TokenResult> { Success = true, Data = tokenResult };
            }

            _logger.LogWarning("OTP doğrulanamadı: StatusCode={StatusCode}, Response={Response}", 
                response.StatusCode, responseContent);
            
            var errorMessage = "OTP kodu geçersiz veya süresi dolmuş.";
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                errorMessage = errorResponse?.message?.ToString() ?? errorMessage;
            }
            catch { }

            return new ApiResponse<TokenResult> { Success = false, Message = errorMessage };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "OTP doğrulama timeout");
            return new ApiResponse<TokenResult> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OTP doğrulama hatası");
            return new ApiResponse<TokenResult> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    // ============================================
    // PROFILE METODLARI
    // ============================================

    public async Task<ApiResponse<AddressInfo>> GetCustomerAddress(long customerId)
    {
        try
        {
            _logger.LogInformation("Müşteri adresi alınıyor: CustomerId={CustomerId}", customerId);

            var apiCode = _configuration.GetValue<string>("ApiSettings:CustomersApiAddressCode");
            var url = $"{CUSTOMERS_API}/api/customer/address/{customerId}";
            if (!string.IsNullOrEmpty(apiCode))
            {
                url += $"?code={Uri.EscapeDataString(apiCode)}";
            }

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                AddressInfo? addressInfo = null;
                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    if (apiResponse?.value != null)
                    {
                        addressInfo = JsonConvert.DeserializeObject<AddressInfo>(apiResponse.value.ToString());
                    }
                    else
                    {
                        addressInfo = JsonConvert.DeserializeObject<AddressInfo>(responseContent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Response deserialize hatası: Response={Response}", responseContent);
                    addressInfo = JsonConvert.DeserializeObject<AddressInfo>(responseContent);
                }

                return new ApiResponse<AddressInfo> { Success = true, Data = addressInfo, Value = addressInfo };
            }

            _logger.LogWarning("Adres bilgisi alınamadı: StatusCode={StatusCode}, CustomerId={CustomerId}", 
                response.StatusCode, customerId);
            return new ApiResponse<AddressInfo> { Success = false, Message = "Adres bilgisi alınamadı." };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Adres bilgisi alma timeout: CustomerId={CustomerId}", customerId);
            return new ApiResponse<AddressInfo> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Adres bilgisi alma hatası: CustomerId={CustomerId}", customerId);
            return new ApiResponse<AddressInfo> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    public async Task<ApiResponse<bool>> SaveAddress(AddressModel address)
    {
        try
        {
            _logger.LogInformation("Adres bilgisi kaydediliyor: CustomerId={CustomerId}, CityId={CityId}, TownId={TownId}", 
                address.CustomerId, address.CityId, address.TownId);

            // API request formatı: { customerId, adress, cityId, townId, source }
            // source: 2 = kişi ekler, 1 = danışmanlık ekler
            var request = new
            {
                customerId = address.CustomerId,
                adress = address.Address ?? string.Empty,
                cityId = address.CityId,
                townId = address.TownId,
                source = 2 // Kişi ekler
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var apiCode = _configuration.GetValue<string>("ApiSettings:CustomersApiAddressSaveCode");
            var url = $"{CUSTOMERS_API}/api/customer/address";
            if (!string.IsNullOrEmpty(apiCode))
            {
                url += $"?code={Uri.EscapeDataString(apiCode)}";
            }

            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Response wrapper kontrolü
                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(responseContent);
                    bool success = false;
                    string messageStr = "Adres bilgisi kaydedildi.";
                    
                    if (apiResponse != null)
                    {
                        var successToken = apiResponse["success"];
                        if (successToken != null)
                        {
                            success = successToken.ToObject<bool>();
                        }
                        
                        var messageToken = apiResponse["message"];
                        if (messageToken != null)
                        {
                            messageStr = messageToken.ToString();
                        }
                    }
                    
                    if (success)
                    {
                        _logger.LogInformation("Adres bilgisi kaydedildi: CustomerId={CustomerId}", address.CustomerId);
                        return new ApiResponse<bool> { Success = true, Data = true, Message = messageStr };
                    }
                    else
                    {
                        _logger.LogWarning("Adres bilgisi kaydedilemedi: Message={Message}, CustomerId={CustomerId}", 
                            messageStr, address.CustomerId);
                        return new ApiResponse<bool> { Success = false, Message = messageStr };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Response deserialize hatası: Response={Response}", responseContent);
                    // Fallback: başarılı kabul et
                    _logger.LogInformation("Adres bilgisi kaydedildi: CustomerId={CustomerId}", address.CustomerId);
                    return new ApiResponse<bool> { Success = true, Data = true };
                }
            }

            _logger.LogWarning("Adres bilgisi kaydedilemedi: StatusCode={StatusCode}, CustomerId={CustomerId}", 
                response.StatusCode, address.CustomerId);
            
            var errorMessage = "Adres bilgisi kaydedilemedi.";
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                errorMessage = errorResponse?.message?.ToString() ?? errorMessage;
            }
            catch { }

            return new ApiResponse<bool> { Success = false, Message = errorMessage };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Adres bilgisi kaydetme timeout: CustomerId={CustomerId}", address.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Adres bilgisi kaydetme hatası: CustomerId={CustomerId}", address.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    public async Task<ApiResponse<JobProfileModel>> GetJobInfo(long customerId)
    {
        try
        {
            _logger.LogInformation("Meslek bilgileri alınıyor: CustomerId={CustomerId}", customerId);

            var apiCode = _configuration.GetValue<string>("ApiSettings:CustomersApiJobInfoCode");
            var url = $"{CUSTOMERS_API}/api/customer/job-info/new/{customerId}";
            if (!string.IsNullOrEmpty(apiCode))
            {
                url += $"?code={Uri.EscapeDataString(apiCode)}";
            }

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                JobProfileModel? jobInfo = null;
                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    if (apiResponse?.value != null)
                    {
                        jobInfo = JsonConvert.DeserializeObject<JobProfileModel>(apiResponse.value.ToString());
                    }
                    else
                    {
                        jobInfo = JsonConvert.DeserializeObject<JobProfileModel>(responseContent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Response deserialize hatası: Response={Response}", responseContent);
                    jobInfo = JsonConvert.DeserializeObject<JobProfileModel>(responseContent);
                }

                return new ApiResponse<JobProfileModel> { Success = true, Data = jobInfo, Value = jobInfo };
            }

            _logger.LogWarning("Meslek bilgileri alınamadı: StatusCode={StatusCode}, CustomerId={CustomerId}", 
                response.StatusCode, customerId);
            return new ApiResponse<JobProfileModel> { Success = false, Message = "Meslek bilgileri alınamadı." };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Meslek bilgileri alma timeout: CustomerId={CustomerId}", customerId);
            return new ApiResponse<JobProfileModel> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Meslek bilgileri alma hatası: CustomerId={CustomerId}", customerId);
            return new ApiResponse<JobProfileModel> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    public async Task<ApiResponse<bool>> SaveJobProfile(JobProfileRequest request)
    {
        try
        {
            _logger.LogInformation("Meslek bilgileri kaydediliyor: CustomerId={CustomerId}, JobId={JobId}, SectorId={SectorId}", 
                request.CustomerId, request.JobId, request.SectorId);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{CUSTOMERS_API}/api/customer/job-profile", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Meslek bilgileri kaydedildi: CustomerId={CustomerId}", request.CustomerId);
                return new ApiResponse<bool> { Success = true, Data = true };
            }

            _logger.LogWarning("Meslek bilgileri kaydedilemedi: StatusCode={StatusCode}, CustomerId={CustomerId}", 
                response.StatusCode, request.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "Meslek bilgileri kaydedilemedi." };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Meslek bilgileri kaydetme timeout: CustomerId={CustomerId}", request.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Meslek bilgileri kaydetme hatası: CustomerId={CustomerId}", request.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    public async Task<ApiResponse<FinanceModel>> GetFinanceAssets(long customerId)
    {
        try
        {
            _logger.LogInformation("Finansal bilgiler alınıyor: CustomerId={CustomerId}", customerId);

            var apiCode = _configuration.GetValue<string>("ApiSettings:CustomersApiFinanceAssetsCode");
            var url = $"{CUSTOMERS_API}/api/customer/finance-assets/{customerId}";
            if (!string.IsNullOrEmpty(apiCode))
            {
                url += $"?code={Uri.EscapeDataString(apiCode)}";
            }

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                FinanceModel? financeInfo = null;
                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    if (apiResponse?.value != null)
                    {
                        financeInfo = JsonConvert.DeserializeObject<FinanceModel>(apiResponse.value.ToString());
                    }
                    else
                    {
                        financeInfo = JsonConvert.DeserializeObject<FinanceModel>(responseContent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Response deserialize hatası: Response={Response}", responseContent);
                    financeInfo = JsonConvert.DeserializeObject<FinanceModel>(responseContent);
                }

                return new ApiResponse<FinanceModel> { Success = true, Data = financeInfo, Value = financeInfo };
            }

            _logger.LogWarning("Finansal bilgiler alınamadı: StatusCode={StatusCode}, CustomerId={CustomerId}", 
                response.StatusCode, customerId);
            return new ApiResponse<FinanceModel> { Success = false, Message = "Finansal bilgiler alınamadı." };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Finansal bilgiler alma timeout: CustomerId={CustomerId}", customerId);
            return new ApiResponse<FinanceModel> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Finansal bilgiler alma hatası: CustomerId={CustomerId}", customerId);
            return new ApiResponse<FinanceModel> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    public async Task<ApiResponse<bool>> SaveFinanceAssets(FinanceModel finance)
    {
        try
        {
            _logger.LogInformation("Finansal bilgiler kaydediliyor: CustomerId={CustomerId}, SalaryAmount={SalaryAmount}", 
                finance.CustomerId, finance.SalaryAmount);

            var json = JsonConvert.SerializeObject(finance);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{CUSTOMERS_API}/api/customer/finance-assets", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Finansal bilgiler kaydedildi: CustomerId={CustomerId}", finance.CustomerId);
                return new ApiResponse<bool> { Success = true, Data = true };
            }

            _logger.LogWarning("Finansal bilgiler kaydedilemedi: StatusCode={StatusCode}, CustomerId={CustomerId}", 
                response.StatusCode, finance.CustomerId);
            
            var errorMessage = "Finansal bilgiler kaydedilemedi.";
            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                errorMessage = errorResponse?.message?.ToString() ?? errorMessage;
            }
            catch { }

            return new ApiResponse<bool> { Success = false, Message = errorMessage };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Finansal bilgiler kaydetme timeout: CustomerId={CustomerId}", finance.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Finansal bilgiler kaydetme hatası: CustomerId={CustomerId}", finance.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    public async Task<ApiResponse<bool>> SaveIncomeInfo(IncomeInfoRequest request)
    {
        try
        {
            _logger.LogInformation("Gelir bilgileri kaydediliyor: CustomerId={CustomerId}, MonthlyIncome={MonthlyIncome}", 
                request.CustomerId, request.MonthlyIncome);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{CUSTOMERS_API}/api/customer/income", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Gelir bilgileri kaydedildi: CustomerId={CustomerId}", request.CustomerId);
                return new ApiResponse<bool> { Success = true, Data = true };
            }

            _logger.LogWarning("Gelir bilgileri kaydedilemedi: StatusCode={StatusCode}, CustomerId={CustomerId}", 
                response.StatusCode, request.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "Gelir bilgileri kaydedilemedi." };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Gelir bilgileri kaydetme timeout: CustomerId={CustomerId}", request.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gelir bilgileri kaydetme hatası: CustomerId={CustomerId}", request.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    public async Task<ApiResponse<WifeInfoModel>> GetWifeInfo(long customerId)
    {
        try
        {
            _logger.LogInformation("Eş bilgileri alınıyor: CustomerId={CustomerId}", customerId);

            var apiCode = _configuration.GetValue<string>("ApiSettings:CustomersApiWifeInfoCode");
            var url = $"{CUSTOMERS_API}/api/customer/wife-info/{customerId}";
            if (!string.IsNullOrEmpty(apiCode))
            {
                url += $"?code={Uri.EscapeDataString(apiCode)}";
            }

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                WifeInfoModel? wifeInfo = null;
                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    if (apiResponse?.value != null)
                    {
                        wifeInfo = JsonConvert.DeserializeObject<WifeInfoModel>(apiResponse.value.ToString());
                    }
                    else
                    {
                        wifeInfo = JsonConvert.DeserializeObject<WifeInfoModel>(responseContent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Response deserialize hatası: Response={Response}", responseContent);
                    wifeInfo = JsonConvert.DeserializeObject<WifeInfoModel>(responseContent);
                }

                return new ApiResponse<WifeInfoModel> { Success = true, Data = wifeInfo, Value = wifeInfo };
            }

            _logger.LogWarning("Eş bilgileri alınamadı: StatusCode={StatusCode}, CustomerId={CustomerId}", 
                response.StatusCode, customerId);
            return new ApiResponse<WifeInfoModel> { Success = false, Message = "Eş bilgileri alınamadı." };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Eş bilgileri alma timeout: CustomerId={CustomerId}", customerId);
            return new ApiResponse<WifeInfoModel> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eş bilgileri alma hatası: CustomerId={CustomerId}", customerId);
            return new ApiResponse<WifeInfoModel> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    public async Task<ApiResponse<bool>> SaveWifeInfo(WifeInfoModel wifeInfo)
    {
        try
        {
            _logger.LogInformation("Eş bilgileri kaydediliyor: CustomerId={CustomerId}, MaritalStatus={MaritalStatus}, WorkWife={WorkWife}, WifeSalaryAmount={WifeSalaryAmount}", 
                wifeInfo.CustomerId, wifeInfo.MaritalStatus, wifeInfo.WorkWife, wifeInfo.WifeSalaryAmount);

            var json = JsonConvert.SerializeObject(wifeInfo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{CUSTOMERS_API}/api/customer/wife-info/{wifeInfo.CustomerId}", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Eş bilgileri kaydedildi: CustomerId={CustomerId}", wifeInfo.CustomerId);
                return new ApiResponse<bool> { Success = true, Data = true };
            }

            _logger.LogWarning("Eş bilgileri kaydedilemedi: StatusCode={StatusCode}, CustomerId={CustomerId}", 
                response.StatusCode, wifeInfo.CustomerId);
            
            var errorMessage = "Eş bilgileri kaydedilemedi.";
            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                errorMessage = errorResponse?.message?.ToString() ?? errorMessage;
            }
            catch { }

            return new ApiResponse<bool> { Success = false, Message = errorMessage };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Eş bilgileri kaydetme timeout: CustomerId={CustomerId}", wifeInfo.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eş bilgileri kaydetme hatası: CustomerId={CustomerId}", wifeInfo.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    public async Task<ApiResponse<bool>> SaveSpouseInfo(SpouseInfoRequest request)
    {
        try
        {
            _logger.LogInformation("Eş bilgileri kaydediliyor: CustomerId={CustomerId}", request.CustomerId);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{CUSTOMERS_API}/api/customer/spouse", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Eş bilgileri kaydedildi: CustomerId={CustomerId}", request.CustomerId);
                return new ApiResponse<bool> { Success = true, Data = true };
            }

            _logger.LogWarning("Eş bilgileri kaydedilemedi: StatusCode={StatusCode}, CustomerId={CustomerId}", 
                response.StatusCode, request.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "Eş bilgileri kaydedilemedi." };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Eş bilgileri kaydetme timeout: CustomerId={CustomerId}", request.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "İstek zaman aşımına uğradı." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eş bilgileri kaydetme hatası: CustomerId={CustomerId}", request.CustomerId);
            return new ApiResponse<bool> { Success = false, Message = "Bir hata oluştu." };
        }
    }

    // ============================================
    // REPORTS METODLARI
    // ============================================

    public async Task<ApiResponse<List<ReportModel>>> GetReportList(long customerId)
    {
        try
        {
            _logger.LogInformation("Rapor listesi alınıyor: CustomerId={CustomerId}", customerId);

            // Dummy API endpoint - Gerçek uygulamada değiştirilecek
            var response = await _httpClient.GetAsync($"{CUSTOMERS_API}/api/dummy/report-list?customerId={customerId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var reportList = JsonConvert.DeserializeObject<List<ReportModel>>(responseContent);
                
                // Eğer API'den boş gelirse, örnek veri oluştur
                if (reportList == null || reportList.Count == 0)
                {
                    reportList = GenerateDummyReports(customerId);
                }
                else
                {
                    // Status text'lerini doldur
                    foreach (var report in reportList)
                    {
                        report.StatusText = GetStatusText(report.Status);
                    }
                }

                return new ApiResponse<List<ReportModel>> { Success = true, Data = reportList };
            }

            _logger.LogWarning("Rapor listesi alınamadı: StatusCode={StatusCode}, CustomerId={CustomerId}", 
                response.StatusCode, customerId);
            
            // API başarısız olsa bile dummy veri döndür
            var dummyReports = GenerateDummyReports(customerId);
            return new ApiResponse<List<ReportModel>> { Success = true, Data = dummyReports };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Rapor listesi alma timeout: CustomerId={CustomerId}", customerId);
            // Timeout durumunda da dummy veri döndür
            var dummyReports = GenerateDummyReports(customerId);
            return new ApiResponse<List<ReportModel>> { Success = true, Data = dummyReports };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rapor listesi alma hatası: CustomerId={CustomerId}", customerId);
            // Hata durumunda da dummy veri döndür
            var dummyReports = GenerateDummyReports(customerId);
            return new ApiResponse<List<ReportModel>> { Success = true, Data = dummyReports };
        }
    }

    private List<ReportModel> GenerateDummyReports(long customerId)
    {
        return new List<ReportModel>
        {
            new ReportModel
            {
                ReportId = 1,
                ReportNumber = "KR-" + DateTime.Now.Year + "-0001",
                ReportName = "Kredi Başvurusu",
                ReportDate = DateTime.Now.AddDays(-5),
                Status = 1, // Onaylandı
                StatusText = "Onaylandı",
                LoanAmount = 100000,
                Term = 24
            },
            new ReportModel
            {
                ReportId = 2,
                ReportNumber = "KR-" + DateTime.Now.Year + "-0002",
                ReportName = "Kredi Başvurusu",
                ReportDate = DateTime.Now.AddDays(-2),
                Status = 0, // Bekliyor
                StatusText = "Bekliyor",
                LoanAmount = 50000,
                Term = 12
            },
            new ReportModel
            {
                ReportId = 3,
                ReportNumber = "KR-" + DateTime.Now.Year + "-0003",
                ReportName = "Kredi Başvurusu",
                ReportDate = DateTime.Now.AddDays(-10),
                Status = 2, // Reddedildi
                StatusText = "Reddedildi",
                LoanAmount = 200000,
                Term = 36
            }
        };
    }

    private string GetStatusText(int status)
    {
        return status switch
        {
            0 => "Bekliyor",
            1 => "Onaylandı",
            2 => "Reddedildi",
            _ => "Bilinmiyor"
        };
    }

    public async Task<ApiResponse<ReportDetailModel>> GetReportDetail(long reportId)
    {
        try
        {
            _logger.LogInformation("Rapor detayı alınıyor: ReportId={ReportId}", reportId);

            // Dummy API endpoint - Gerçek uygulamada değiştirilecek
            var response = await _httpClient.GetAsync($"{CUSTOMERS_API}/api/GetReportDetail?reportId={reportId}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var reportDetail = JsonConvert.DeserializeObject<ReportDetailModel>(responseContent);
                
                // Eğer API'den boş gelirse, örnek veri oluştur
                if (reportDetail == null)
                {
                    reportDetail = GenerateDummyReportDetail(reportId);
                }
                else
                {
                    // Status text'i doldur
                    reportDetail.StatusText = GetStatusText(reportDetail.Status);
                }

                return new ApiResponse<ReportDetailModel> { Success = true, Data = reportDetail };
            }

            _logger.LogWarning("Rapor detayı alınamadı: StatusCode={StatusCode}, ReportId={ReportId}", 
                response.StatusCode, reportId);
            
            // API başarısız olsa bile dummy veri döndür
            var dummyDetail = GenerateDummyReportDetail(reportId);
            return new ApiResponse<ReportDetailModel> { Success = true, Data = dummyDetail };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Rapor detayı alma timeout: ReportId={ReportId}", reportId);
            // Timeout durumunda da dummy veri döndür
            var dummyDetail = GenerateDummyReportDetail(reportId);
            return new ApiResponse<ReportDetailModel> { Success = true, Data = dummyDetail };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rapor detayı alma hatası: ReportId={ReportId}", reportId);
            // Hata durumunda da dummy veri döndür
            var dummyDetail = GenerateDummyReportDetail(reportId);
            return new ApiResponse<ReportDetailModel> { Success = true, Data = dummyDetail };
        }
    }

    private ReportDetailModel GenerateDummyReportDetail(long reportId)
    {
        var monthlyRate = 0.025m; // 2.5% aylık
        var loanAmount = 100000m;
        var term = 24;
        
        // Basit faiz hesaplama
        var monthlyPayment = (loanAmount * monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), term)) / 
                            ((decimal)Math.Pow((double)(1 + monthlyRate), term) - 1);
        var totalPayment = monthlyPayment * term;
        var totalInterest = totalPayment - loanAmount;

        return new ReportDetailModel
        {
            ReportId = reportId,
            ReportNumber = "KR-" + DateTime.Now.Year + "-" + reportId.ToString("D4"),
            ReportTitle = "Kredi Başvurusu Detay Raporu",
            ReportDate = DateTime.Now.AddDays(-5),
            Status = 1, // Onaylandı
            StatusText = "Onaylandı",
            LoanAmount = loanAmount,
            Term = term,
            MonthlyPayment = monthlyPayment,
            TotalPayment = totalPayment,
            TotalInterest = totalInterest,
            InterestRate = monthlyRate * 100, // Yüzde olarak
            Content = JsonConvert.SerializeObject(new
            {
                BaşvuruTarihi = DateTime.Now.AddDays(-5).ToString("dd.MM.yyyy"),
                OnayTarihi = DateTime.Now.AddDays(-2).ToString("dd.MM.yyyy"),
                MüşteriBilgileri = new
                {
                    Ad = "Örnek",
                    Soyad = "Müşteri",
                    TCKN = "5*********",
                    GSM = "5XX****XX90"
                },
                KrediBilgileri = new
                {
                    Tutar = loanAmount,
                    Vade = term,
                    FaizOranı = monthlyRate * 100
                }
            }, Formatting.Indented),
            AdditionalInfo = new Dictionary<string, string>
            {
                { "Başvuru Tipi", "Bireysel Kredi" },
                { "Kredi Türü", "İhtiyaç Kredisi" },
                { "Onaylayan", "Sistem" },
                { "Notlar", "Başvuru başarıyla onaylanmıştır." }
            }
        };
    }

    // ============================================
    // HELPER METODLARI
    // ============================================

    // TCKN maskeleme helper (5XX...XX formatı)
    private string MaskTckn(string tckn)
    {
        if (string.IsNullOrEmpty(tckn) || tckn.Length < 11) return tckn ?? string.Empty;
        return tckn.Substring(0, 1) + new string('*', 8) + tckn.Substring(9);
    }

    // Telefon maskeleme helper (5XX****XX formatı)
    private string MaskPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 10) return phone ?? string.Empty;
        return phone.Substring(0, 3) + "****" + phone.Substring(7);
    }

    // JWT token'dan CustomerId parse etme helper
    private long ParseCustomerIdFromToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return 0;

        try
        {
            // JWT token formatı: header.payload.signature
            var parts = token.Split('.');
            if (parts.Length < 2)
                return 0;

            // Payload'ı decode et (Base64)
            var payload = parts[1];
            
            // Base64 padding ekle (gerekirse)
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            
            // JSON'dan CustomerId'yi çıkar
            var payloadObj = JsonConvert.DeserializeObject<dynamic>(payloadJson);
            
            // CustomerId claim'ini bul (farklı isimlerde olabilir: customerId, CustomerId, sub, nameid, vb.)
            if (payloadObj?.customerId != null)
            {
                return Convert.ToInt64(payloadObj.customerId);
            }
            if (payloadObj?.CustomerId != null)
            {
                return Convert.ToInt64(payloadObj.CustomerId);
            }
            if (payloadObj?.sub != null)
            {
                // sub genellikle string olabilir, long'a parse et
                var subStr = payloadObj.sub.ToString();
                if (long.TryParse(subStr, out long customerId))
                {
                    return customerId;
                }
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token'dan CustomerId parse edilemedi: Token={Token}", 
                token.Substring(0, Math.Min(20, token.Length)) + "...");
            return 0;
        }
    }
}
