using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages;

public class OpportunitiesModel : PageModel
{
    private readonly ILogger<OpportunitiesModel> _logger;

    public OpportunitiesModel(ILogger<OpportunitiesModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        _logger.LogInformation("Yılın Son Fırsatları sayfası yüklendi");
    }
}

