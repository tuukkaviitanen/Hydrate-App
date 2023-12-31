﻿using Hydrate_App.Services;
using Hydrate_App.ViewModels;
using Hydrate_App.Views;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;

namespace Hydrate_App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseLocalNotification()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif
		builder.Services
            .AddSingleton<HydrationNotificationService>()

            .AddSingleton<HydrateViewModel>()

			.AddSingleton<SettingsPage>();

        return builder.Build();
	}
}
