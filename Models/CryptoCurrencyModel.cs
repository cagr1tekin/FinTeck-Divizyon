using Newtonsoft.Json;

namespace InteraktifKredi.Models;

/// <summary>
/// CoinMarketCap API response modeli - Kripto para için
/// </summary>
public class CryptoCurrencyResponseModel
{
    public CryptoCurrencyData? Data { get; set; }
    public CryptoStatus? Status { get; set; }
}

public class CryptoCurrencyData
{
    [JsonProperty("BTC")]
    public CryptoCurrencyInfo? BTC { get; set; }
    
    [JsonProperty("ETH")]
    public CryptoCurrencyInfo? ETH { get; set; }
    
    [JsonProperty("BNB")]
    public CryptoCurrencyInfo? BNB { get; set; }
    
    [JsonProperty("SOL")]
    public CryptoCurrencyInfo? SOL { get; set; }
}

public class CryptoCurrencyInfo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Symbol { get; set; }
    public CryptoQuote? Quote { get; set; }
}

public class CryptoQuote
{
    [JsonProperty("TRY")]
    public CryptoCurrencyPrice? TRY { get; set; }
}

public class CryptoCurrencyPrice
{
    public decimal Price { get; set; }
    
    [JsonProperty("percent_change_24h")]
    public decimal? PercentChange24h { get; set; }
    
    [JsonProperty("last_updated")]
    public DateTime? LastUpdated { get; set; }
}

public class CryptoStatus
{
    [JsonProperty("error_code")]
    public int ErrorCode { get; set; }
    
    [JsonProperty("error_message")]
    public string? ErrorMessage { get; set; }
}

