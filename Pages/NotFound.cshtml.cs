using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InteraktifKredi.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class NotFoundModel : PageModel
{
    private readonly ILogger<NotFoundModel> _logger;

    public NotFoundModel(ILogger<NotFoundModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        Response.StatusCode = 404;
        _logger.LogWarning("404 sayfası görüntülendi: Path={Path}", HttpContext.Request.Path);
    }
}

