namespace InteraktifKredi.Models;

/// <summary>
/// Eş bilgileri modeli
/// NOT: Tüm parasal değerler decimal tipindedir (float/double YASAK!)
/// </summary>
public class WifeInfoModel
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
    /// Medeni durum (evli mi?) - nullable
    /// </summary>
    public bool? MaritalStatus { get; set; }

    /// <summary>
    /// Eş çalışıyor mu? - nullable
    /// </summary>
    public bool? WorkWife { get; set; }

    /// <summary>
    /// Eş maaşı (decimal - nullable)
    /// </summary>
    public decimal? WifeSalaryAmount { get; set; }
}

