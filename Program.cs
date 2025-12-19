using InteraktifKredi.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var razorPagesBuilder = builder.Services.AddRazorPages();

// RuntimeCompilation sadece Development ortamında etkin
if (builder.Environment.IsDevelopment())
{
    razorPagesBuilder.AddRazorRuntimeCompilation();
}

// Session
var sessionTimeout = builder.Configuration.GetValue<int>("Session:IdleTimeout", 30);
var cookieSecure = builder.Configuration.GetValue<bool>("Session:CookieSecure", !builder.Environment.IsDevelopment());

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(sessionTimeout);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = cookieSecure ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
});

// HttpClient ve API Service
builder.Services.AddHttpClient<IApiService, ApiService>();

// Türkçe karakter desteği için encoding ayarları
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

var app = builder.Build();

// Forwarded Headers (for reverse proxy/load balancer)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    
    // HSTS Configuration
    var hstsMaxAge = app.Configuration.GetValue<int>("Hsts:MaxAge", 31536000); // 1 year default
    var hstsIncludeSubDomains = app.Configuration.GetValue<bool>("Hsts:IncludeSubDomains", true);
    var hstsPreload = app.Configuration.GetValue<bool>("Hsts:Preload", true);
    
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Status code pages (404, 500, etc.)
app.UseStatusCodePagesWithReExecute("/NotFound");

app.UseHttpsRedirection();

// Security Headers
app.Use(async (context, next) =>
{
    // X-Content-Type-Options
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    
    // X-Frame-Options
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    
    // X-XSS-Protection
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    
    // Referrer-Policy
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // Content-Security-Policy (temel)
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Content-Security-Policy", 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
            "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
            "font-src 'self' https://fonts.gstatic.com; " +
            "img-src 'self' data: https:; " +
            "connect-src 'self' https://customers-api.azurewebsites.net https://api-idc.azurewebsites.net;");
    }
    
    await next();
});

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 1 year in production
        if (!app.Environment.IsDevelopment())
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000,immutable");
        }
    }
});

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
