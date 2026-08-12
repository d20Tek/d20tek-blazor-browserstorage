using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using D20Tek.Blazor.BrowserStorage;
using PreferenceDashboard;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBrowserStorage(options =>
{
    options.KeyPrefix = "prefs_";
});

builder.Services.AddScoped<PreferenceDashboard.Services.PreferenceService>();

await builder.Build().RunAsync();
