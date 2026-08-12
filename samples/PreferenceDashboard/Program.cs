using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using D20Tek.Blazor.BrowserStorage;
using PreferenceDashboard;
using PreferenceDashboard.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBrowserStorage(options =>
    {
        options.KeyPrefix = "sample:";
    }, ServiceLifetime.Singleton);

builder.Services.AddSingleton<PreferenceService>();

await builder.Build().RunAsync();
