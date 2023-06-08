using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Hydrate_App.ViewModels;

public partial class HydrateViewModel : BaseViewModel
{
    public int MinimumHydrateInterval => 5;
    public int MaximumHydrateInterval => 120;

    [ObservableProperty]
    TimeSpan hydrateTimer;

    int hydrateIntervalInMinutes;
    public int HydrateIntervalInMinutes
    {
        get => hydrateIntervalInMinutes;
        set => SetProperty(ref hydrateIntervalInMinutes, (int)(Math.Round(value / (double)MinimumHydrateInterval) * MinimumHydrateInterval));
    }

    public HydrateViewModel()
    {
        Title = "Hydrate";
    }

    [RelayCommand]
    public async Task ToggleHydrationTimerAsync()
    {

    }
}
