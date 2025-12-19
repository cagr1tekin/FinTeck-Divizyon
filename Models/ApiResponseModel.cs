namespace InteraktifKredi.Models;

/// <summary>
/// API yanıt modeli - Generic tip desteği
/// </summary>
/// <typeparam name="T">Yanıt veri tipi</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// HTTP Status Code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Hata veya bilgi mesajı
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Yanıt verisi (generic)
    /// </summary>
    public T? Value { get; set; }

    /// <summary>
    /// Data property (Value için alias - geriye dönük uyumluluk)
    /// </summary>
    public T? Data
    {
        get => Value;
        set => Value = value;
    }
}
