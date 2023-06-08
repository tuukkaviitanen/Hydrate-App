using CommunityToolkit.Mvvm.ComponentModel;

namespace Hydrate_App.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    string title;

    [ObservableProperty]
    bool isBusy;

}
