using InteraktifKredi.Models;

namespace InteraktifKredi.Services;

public interface IApiService
{
    // Onboarding metodları
    Task<ApiResponse<CustomerInfo>> ValidateTcknGsm(string tckn, string gsm);
    Task<ApiResponse<string>> GetKvkkText(int kvkkId);
    Task<ApiResponse<bool>> SaveKvkkOnay(int kvkkId, long customerId);
    Task<ApiResponse<OtpResult>> GenerateOtp(string tckn, string gsm);
    Task<ApiResponse<bool>> SendOtpSms(string gsm, string otpCode);
    Task<ApiResponse<TokenResult>> VerifyOtp(string otpCode);

    // Profile metodları
    Task<ApiResponse<AddressInfo>> GetCustomerAddress(long customerId);
    Task<ApiResponse<bool>> SaveAddress(AddressModel address);
    Task<ApiResponse<JobProfileModel>> GetJobInfo(long customerId);
    Task<ApiResponse<bool>> SaveJobProfile(JobProfileRequest request);
    Task<ApiResponse<FinanceModel>> GetFinanceAssets(long customerId);
    Task<ApiResponse<bool>> SaveFinanceAssets(FinanceModel finance);
    Task<ApiResponse<WifeInfoModel>> GetWifeInfo(long customerId);
    Task<ApiResponse<bool>> SaveWifeInfo(WifeInfoModel wifeInfo);
    Task<ApiResponse<bool>> SaveIncomeInfo(IncomeInfoRequest request);
    Task<ApiResponse<bool>> SaveSpouseInfo(SpouseInfoRequest request);

    // Reports metodları
    Task<ApiResponse<List<ReportModel>>> GetReportList(long customerId);
    Task<ApiResponse<ReportDetailModel>> GetReportDetail(long reportId);

    // TurkeyAPI metodları
    Task<ApiResponse<List<ProvinceModel>>> GetProvinces();
    Task<ApiResponse<List<DistrictModel>>> GetDistrictsByProvinceId(int provinceId);
}
