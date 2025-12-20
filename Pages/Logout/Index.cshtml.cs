using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages.Logout;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        // Session'dan tüm login bilgilerini temizle
        var customerId = HttpContext.Session.GetString("CustomerId");
        var customerName = HttpContext.Session.GetString("CustomerName");
        var tckn = HttpContext.Session.GetString("TCKN");
        var gsm = HttpContext.Session.GetString("GSM");

        // Session'ı temizle
        HttpContext.Session.Clear();

        _logger.LogInformation("Kullanıcı çıkış yaptı: CustomerId={CustomerId}, CustomerName={CustomerName}, TCKN={MaskedTckn}",
            customerId, customerName, MaskTckn(tckn));

        // Login sayfasına yönlendir
        return RedirectToPage("/Onboarding/TcknGsm");
    }

    private string MaskTckn(string? tckn)
    {
        if (string.IsNullOrEmpty(tckn) || tckn.Length < 4)
            return "****";
        
        return tckn.Substring(0, 3) + "****" + tckn.Substring(tckn.Length - 2);
    }
}

