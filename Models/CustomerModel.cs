namespace InteraktifKredi.Models;

/// <summary>
/// Müşteri bilgileri modeli
/// </summary>
public class CustomerInfo
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
    /// GSM numarası
    /// </summary>
    public string? GSM { get; set; }

    /// <summary>
    /// E-posta adresi
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Ad
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Soyad
    /// </summary>
    public string? Surname { get; set; }

    // Legacy properties (geriye dönük uyumluluk için)
    public string? Tckn
    {
        get => TCKN;
        set => TCKN = value;
    }

    public string? Gsm
    {
        get => GSM;
        set => GSM = value;
    }
}

// Onboarding modelleri
public class OtpResult
{
    public string OtpCode { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int RetryCount { get; set; }
}

public class TokenResult
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public long CustomerId { get; set; }
}

public class KvkkOnayRequest
{
    public int KvkkId { get; set; }
    public long CustomerId { get; set; }
    public bool Accepted { get; set; }
    public DateTime AcceptedAt { get; set; }
}

// Legacy models (geriye dönük uyumluluk için)
public class AddressInfo
{
    public string? TCKN { get; set; }
    public long CustomerId { get; set; }
    public int? CityId { get; set; }
    public int? TownId { get; set; }
    public string? Address { get; set; }
    public int? EmployeeId { get; set; }
    public string? Source { get; set; }
    public DateTime? CreateDate { get; set; }

    // Legacy properties (geriye dönük uyumluluk için)
    public string? AddressLine { get => Address; set => Address = value; }
    public string? City { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
}

public class JobProfileRequest
{
    public long CustomerId { get; set; }
    public int JobId { get; set; }
    public int SectorId { get; set; }
}

public class IncomeInfoRequest
{
    public long CustomerId { get; set; }
    public decimal MonthlyIncome { get; set; }
}

public class SpouseInfoRequest
{
    public long CustomerId { get; set; }
    public string SpouseName { get; set; } = string.Empty;
    public string SpouseSurname { get; set; } = string.Empty;
    public string SpouseTckn { get; set; } = string.Empty;
}
