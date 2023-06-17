using CommunityToolkit.Mvvm.ComponentModel;

namespace Hydrate_App.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    /// <summary>
    /// Title of the ViewModel that can be read from View
    /// </summary>
    [ObservableProperty]
    string title;

    /// <summary>
    /// A simple lock to disallow many requests of the same methods at the same time
    /// </summary>
    [ObservableProperty]
    bool isBusy;

    /// <summary>
    /// Base
    /// </summary>
    /// <param name="title">Sets the title of the ViewModel</param>
    public BaseViewModel(string title)
    {
        Title = title;
    }

}
