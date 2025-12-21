namespace InteraktifKredi.Models;

/// <summary>
/// Döviz kuru response modeli - doviz.dev API için
/// </summary>
public class CurrencyResponseModel
{
    // Ana kurlar (dinamik olarak gelir)
    public decimal? USDTRY { get; set; }
    public decimal? TRYUSD { get; set; }
    public decimal? EURTRY { get; set; }
    public decimal? TRYEUR { get; set; }
    public decimal? GBPTRY { get; set; }
    public decimal? TRYGBP { get; set; }
    public decimal? CHFTRY { get; set; }
    public decimal? TRYCHF { get; set; }
    public decimal? JPYTRY { get; set; }
    public decimal? TRYJPY { get; set; }
    public decimal? AUDTRY { get; set; }
    public decimal? TRYAUD { get; set; }
    public decimal? DKKTRY { get; set; }
    public decimal? TRYDKK { get; set; }
    public decimal? CADTRY { get; set; }
    public decimal? TRYCAD { get; set; }
    public decimal? PLNTRY { get; set; }
    public decimal? TRYPLN { get; set; }
    public decimal? TRYTRY { get; set; }
    
    // Meta bilgileri
    public CurrencyMeta? _meta { get; set; }
}

public class CurrencyMeta
{
    public string? Base { get; set; }
    public string? Source { get; set; }
    public DateTime? GeneratedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

