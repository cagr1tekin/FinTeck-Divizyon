using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages;

public class KrediKartiModel : PageModel
{
    private readonly ILogger<KrediKartiModel> _logger;

    public KrediKartiModel(ILogger<KrediKartiModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        _logger.LogInformation("Kredi Kartı sayfası yüklendi");
    }
}

