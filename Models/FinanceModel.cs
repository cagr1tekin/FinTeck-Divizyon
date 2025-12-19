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
    /// Çalışma sektörü ID
    /// </summary>
    public int WorkSector { get; set; }

    /// <summary>
    /// Maaş bankası
    /// </summary>
    public string? SalaryBank { get; set; }

    /// <summary>
    /// Maaş miktarı (decimal - ZORUNLU!)
    /// </summary>
    public decimal SalaryAmount { get; set; }

    /// <summary>
    /// Araba durumu (var mı?)
    /// </summary>
    public bool CarStatus { get; set; }

    /// <summary>
    /// Ev durumu (var mı?)
    /// </summary>
    public bool HouseStatus { get; set; }
}

