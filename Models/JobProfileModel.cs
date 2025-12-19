namespace InteraktifKredi.Models;

/// <summary>
/// Meslek profili modeli
/// </summary>
public class JobProfileModel
{
    /// <summary>
    /// Müşteri ID
    /// </summary>
    public long CustomerId { get; set; }

    /// <summary>
    /// Meslek grubu ID
    /// </summary>
    public int JobGroupId { get; set; }

    /// <summary>
    /// Müşteri çalışma durumu (1: Çalışıyor, 0: Çalışmıyor)
    /// </summary>
    public int CustomerWork { get; set; }

    /// <summary>
    /// Çalışma yılı
    /// </summary>
    public int WorkingYears { get; set; }

    /// <summary>
    /// Çalışma ayı (0-11)
    /// </summary>
    public int WorkingMonth { get; set; }

    /// <summary>
    /// Şirket unvanı
    /// </summary>
    public string? TitleCompany { get; set; }

    /// <summary>
    /// Şirketteki pozisyonu
    /// </summary>
    public string? CompanyPosition { get; set; }
}

