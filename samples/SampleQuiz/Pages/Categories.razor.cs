using Microsoft.AspNetCore.Components;
using SampleQuiz.Services;

namespace SampleQuiz.Pages;

public partial class Categories
{
    private List<string> _unlockedCategories = [];
    private bool _loading = true;

    [Inject]
    private GameService Game { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _unlockedCategories = await Game.GetCategoriesUnlockedAsync();
        _loading = false;
    }
}
