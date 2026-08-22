using D20Tek.Blazor.BrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PreferenceDashboard;
using PreferenceDashboard.Services;
using System.Diagnostics.CodeAnalysis;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBrowserStorage(options =>
    {
        options.KeyPrefix = "sample:";
    }, ServiceLifetime.Singleton);

builder.Services.AddSingleton<PreferenceService>();

await builder.Build().RunAsync();

[ExcludeFromCodeCoverage]
public partial class Program { }
