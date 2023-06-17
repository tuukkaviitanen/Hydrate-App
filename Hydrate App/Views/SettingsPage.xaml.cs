using Hydrate_App.ViewModels;

namespace Hydrate_App.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(HydrateViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}