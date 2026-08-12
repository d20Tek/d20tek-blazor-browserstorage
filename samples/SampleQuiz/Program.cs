using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using D20Tek.Blazor.BrowserStorage;
using SampleQuiz;
using SampleQuiz.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBrowserStorage(options =>
    {
        options.KeyPrefix = "quiz:";
    }, ServiceLifetime.Singleton);

builder.Services.AddSingleton<GameService>();

await builder.Build().RunAsync();
