namespace InteraktifKredi.Models;

/// <summary>
/// SSS (Sık Sorulan Sorular) modeli
/// </summary>
public class FaqItem
{
    /// <summary>
    /// SSS ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Soru
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// Cevap
    /// </summary>
    public string Answer { get; set; } = string.Empty;

    /// <summary>
    /// Sıralama (gösterim sırası)
    /// </summary>
    public int Order { get; set; }
}

