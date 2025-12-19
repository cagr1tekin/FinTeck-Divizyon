namespace InteraktifKredi.Models;

/// <summary>
/// Rapor detay modeli
/// </summary>
public class ReportDetailModel
{
    /// <summary>
    /// Rapor ID
    /// </summary>
    public long ReportId { get; set; }

    /// <summary>
    /// Rapor numarası
    /// </summary>
    public string ReportNumber { get; set; } = string.Empty;

    /// <summary>
    /// Rapor başlığı
    /// </summary>
    public string ReportTitle { get; set; } = string.Empty;

    /// <summary>
    /// Rapor tarihi
    /// </summary>
    public DateTime ReportDate { get; set; }

    /// <summary>
    /// Rapor durumu (0: Bekliyor, 1: Onaylandı, 2: Reddedildi)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Durum metni
    /// </summary>
    public string StatusText { get; set; } = string.Empty;

    /// <summary>
    /// Kredi tutarı (decimal - ZORUNLU!)
    /// </summary>
    public decimal LoanAmount { get; set; }

    /// <summary>
    /// Vade (ay)
    /// </summary>
    public int Term { get; set; }

    /// <summary>
    /// Aylık taksit (decimal - ZORUNLU!)
    /// </summary>
    public decimal MonthlyPayment { get; set; }

    /// <summary>
    /// Toplam geri ödeme (decimal - ZORUNLU!)
    /// </summary>
    public decimal TotalPayment { get; set; }

    /// <summary>
    /// Toplam faiz (decimal - ZORUNLU!)
    /// </summary>
    public decimal TotalInterest { get; set; }

    /// <summary>
    /// Faiz oranı (aylık %)
    /// </summary>
    public decimal InterestRate { get; set; }

    /// <summary>
    /// Rapor içeriği (JSON veya HTML)
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Ek bilgiler (key-value pairs)
    /// </summary>
    public Dictionary<string, string>? AdditionalInfo { get; set; }
}

