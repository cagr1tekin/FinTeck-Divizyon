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
    /// Medeni durum (evli mi?)
    /// </summary>
    public bool MaritalStatus { get; set; }

    /// <summary>
    /// Eş çalışıyor mu?
    /// </summary>
    public bool WorkWife { get; set; }

    /// <summary>
    /// Eş maaşı (decimal - ZORUNLU!)
    /// </summary>
    public decimal WifeSalaryAmount { get; set; }
}

