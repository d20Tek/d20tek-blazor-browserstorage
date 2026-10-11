using System.Diagnostics.CodeAnalysis;
using CookieConsent;
using CookieConsent.Services;
using D20Tek.Blazor.BrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddCookieStorage(
    configure: options =>
    {
        options.KeyPrefix = "sample:";
    },
    configureCookie: cookieOptions =>
    {
        cookieOptions.DefaultExpiration = TimeSpan.FromDays(365);
        cookieOptions.Path = "/";
        cookieOptions.SameSite = CookieSameSiteMode.Lax;
    },
    lifetime: ServiceLifetime.Singleton);

builder.Services.AddSingleton<ConsentBannerService>();

await builder.Build().RunAsync();

[ExcludeFromCodeCoverage]
public partial class Program { }
