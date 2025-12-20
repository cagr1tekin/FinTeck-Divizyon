namespace InteraktifKredi.Models;

/// <summary>
/// Finansal bilgiler modeli
/// NOT: Tüm parasal değerler decimal tipindedir (float/double YASAK!)
/// </summary>
public class FinanceModel
{
    /// <summary>
    /// Müşteri ID
    /// </summary>
    public long CustomerId { get; set; }

    /// <summary>
    /// TC Kimlik No
    /// </summary>
    public string? TCKN { get; set; }

    /// <summary>
    /// Meslek grubu adı
    /// </summary>
    public string? JobGroupName { get; set; }

    /// <summary>
    /// Maaş bankası
    /// </summary>
    public string? SalaryBank { get; set; }

    /// <summary>
    /// Maaş miktarı (decimal - nullable)
    /// </summary>
    public decimal? SalaryAmount { get; set; }

    /// <summary>
    /// Araba durumu (var mı?)
    /// </summary>
    public bool? CarStatus { get; set; }

    /// <summary>
    /// Ev durumu (var mı?)
    /// </summary>
    public bool? HouseStatus { get; set; }

    // Legacy properties (geriye dönük uyumluluk için)
    public int WorkSector { get; set; }
}

