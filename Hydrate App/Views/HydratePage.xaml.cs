using Hydrate_App.ViewModels;

namespace Hydrate_App.Views;

public partial class HydratePage : ContentPage
{
	public HydratePage(HydrateViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}