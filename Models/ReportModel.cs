namespace InteraktifKredi.Models;

/// <summary>
/// Rapor modeli
/// </summary>
public class ReportModel
{
    /// <summary>
    /// Rapor ID
    /// </summary>
    public long ReportId { get; set; }

    /// <summary>
    /// Rapor numarası/başvuru numarası
    /// </summary>
    public string ReportNumber { get; set; } = string.Empty;

    /// <summary>
    /// Rapor adı/başlık
    /// </summary>
    public string ReportName { get; set; } = string.Empty;

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
}

