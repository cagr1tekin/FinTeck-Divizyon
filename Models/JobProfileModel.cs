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
    /// Meslek grubu adı
    /// </summary>
    public string? JobGroupName { get; set; }

    /// <summary>
    /// Meslek adı
    /// </summary>
    public string? OccupationName { get; set; }

    /// <summary>
    /// Şirket unvanı
    /// </summary>
    public string? TitleCompany { get; set; }

    /// <summary>
    /// Şirketteki pozisyonu
    /// </summary>
    public string? CompanyPosition { get; set; }

    /// <summary>
    /// Çalışma yılı
    /// </summary>
    public int? WorkingYears { get; set; }

    /// <summary>
    /// Çalışma ayı (0-11)
    /// </summary>
    public int? WorkingMonth { get; set; }

    // Legacy properties (geriye dönük uyumluluk için)
    public int JobGroupId { get; set; }
    public int CustomerWork { get; set; }
}

