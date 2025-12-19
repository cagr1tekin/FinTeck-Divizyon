namespace InteraktifKredi.Models;

/// <summary>
/// Adres bilgileri modeli
/// </summary>
public class AddressModel
{
    /// <summary>
    /// Müşteri ID
    /// </summary>
    public long CustomerId { get; set; }

    /// <summary>
    /// İl ID
    /// </summary>
    public int CityId { get; set; }

    /// <summary>
    /// İlçe ID
    /// </summary>
    public int TownId { get; set; }

    /// <summary>
    /// Adres satırı (mahalle, sokak, bina no, daire no)
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Posta kodu (opsiyonel)
    /// </summary>
    public string? PostalCode { get; set; }
}

