namespace InteraktifKredi.Models;

/// <summary>
/// Rapor detay modeli - Dashboard için yeni yapı
/// </summary>
public class ReportDetailModel
{
    /// <summary>
    /// Rapor ID
    /// </summary>
    public long ReportId { get; set; }

    /// <summary>
    /// Rapor numarası (referansNo)
    /// </summary>
    public string ReportNumber { get; set; } = string.Empty;

    /// <summary>
    /// Rapor tarihi
    /// </summary>
    public DateTime ReportDate { get; set; }

    // ÖZET KARTLAR (Scoreboard)
    /// <summary>
    /// Kredi notu
    /// </summary>
    public string CreditScore { get; set; } = "0";

    /// <summary>
    /// Toplam limit
    /// </summary>
    public decimal TotalLimit { get; set; }

    /// <summary>
    /// Toplam risk (borç)
    /// </summary>
    public decimal TotalRisk { get; set; }

    /// <summary>
    /// Gecikmedeki toplam hesap sayısı
    /// </summary>
    public int DelayedAccountCount { get; set; }

    /// <summary>
    /// Toplam kredili hesap sayısı
    /// </summary>
    public int TotalCreditAccountCount { get; set; }

    // KREDİ NOTU ANALİZİ
    /// <summary>
    /// Kredi notu sebep kodu 1
    /// </summary>
    public string? CreditScoreReasonCode1 { get; set; }

    /// <summary>
    /// Kredi notu sebep kodu 2
    /// </summary>
    public string? CreditScoreReasonCode2 { get; set; }

    /// <summary>
    /// Kredi notu sebep kodu 3
    /// </summary>
    public string? CreditScoreReasonCode3 { get; set; }

    /// <summary>
    /// Kredi notu sebep kodu 4
    /// </summary>
    public string? CreditScoreReasonCode4 { get; set; }

    /// <summary>
    /// Tarihsel en kötü ödeme durumu (bkWorstPaymetStatusEver)
    /// </summary>
    public int WorstPaymentStatusEver { get; set; }

    /// <summary>
    /// Mevcut en uzun gecikme süresi (ay) (bkMevcutEnUzunGecikmeSuresi)
    /// </summary>
    public int CurrentLongestDelayMonths { get; set; }

    /// <summary>
    /// Son kredi kullanım tarihi (bkSonKrediKullandirimTarihi)
    /// </summary>
    public DateTime? LastCreditUsageDate { get; set; }

    /// <summary>
    /// Sorgu numarası (bkSorguNo)
    /// </summary>
    public string? QueryNumber { get; set; }

    /// <summary>
    /// Hariç tutma kodu (bkExclusionCode)
    /// </summary>
    public string? ExclusionCode { get; set; }

    // BİREYSEL DETAYLAR
    /// <summary>
    /// Aktif krediler listesi
    /// </summary>
    public List<CreditDetail> ActiveCredits { get; set; } = new();

    /// <summary>
    /// Kapanmış krediler listesi
    /// </summary>
    public List<CreditDetail> ClosedCredits { get; set; } = new();

    /// <summary>
    /// Ek bilgiler
    /// </summary>
    public Dictionary<string, string>? AdditionalInfo { get; set; }
}

/// <summary>
/// Kredi detay modeli
/// </summary>
public class CreditDetail
{
    /// <summary>
    /// Sıra numarası
    /// </summary>
    public string SequenceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Kurum rumuzu
    /// </summary>
    public string InstitutionCode { get; set; } = string.Empty;

    /// <summary>
    /// Kredi türü
    /// </summary>
    public string CreditType { get; set; } = string.Empty;

    /// <summary>
    /// Açılış tarihi
    /// </summary>
    public DateTime? OpeningDate { get; set; }

    /// <summary>
    /// Kapanış tarihi (null ise aktif)
    /// </summary>
    public DateTime? ClosingDate { get; set; }

    /// <summary>
    /// Kredi limiti
    /// </summary>
    public decimal CreditLimit { get; set; }

    /// <summary>
    /// Toplam bakiye
    /// </summary>
    public decimal TotalBalance { get; set; }

    /// <summary>
    /// Gecikmedeki bakiye
    /// </summary>
    public decimal DelayedBalance { get; set; }

    /// <summary>
    /// Limit kullanım oranı (0-1 arası)
    /// </summary>
    public decimal LimitUsageRatio { get; set; }

    /// <summary>
    /// Ödeme performans tarihçesi (timeline için)
    /// </summary>
    public string PaymentHistory { get; set; } = string.Empty;

    /// <summary>
    /// Kayıt referans numarası
    /// </summary>
    public string RecordReferenceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Döviz kodu
    /// </summary>
    public string CurrencyCode { get; set; } = "TL";

    /// <summary>
    /// Toplam geciktirilmiş ödeme sayısı
    /// </summary>
    public int TotalDelayedPaymentCount { get; set; }

    /// <summary>
    /// Takibe alınma bakiyesi
    /// </summary>
    public decimal CollectionBalance { get; set; }
}
