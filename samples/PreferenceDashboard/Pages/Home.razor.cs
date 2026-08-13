using D20Tek.Blazor.BrowserStorage;
using Microsoft.AspNetCore.Components;

namespace PreferenceDashboard.Pages;

public partial class Home
{
    private const string BannerDismissedKey = "whats-new-dismissed";
    private bool _bannerDismissed;

    [Inject]
    private ISessionStorageService SessionStorage { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        var result = await SessionStorage.GetAsync<bool>(BannerDismissedKey);
        _bannerDismissed = result.IsSuccess && result.Value;
    }

    private async Task DismissBanner()
    {
        _bannerDismissed = true;
        await SessionStorage.SetAsync(BannerDismissedKey, true);
    }
}
