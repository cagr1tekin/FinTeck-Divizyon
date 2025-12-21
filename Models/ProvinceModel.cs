namespace InteraktifKredi.Models;

/// <summary>
/// İl (Province) modeli - TurkeyAPI için
/// </summary>
public class ProvinceModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// İlçe (District) modeli - TurkeyAPI için
/// </summary>
public class DistrictModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ProvinceId { get; set; }
}

