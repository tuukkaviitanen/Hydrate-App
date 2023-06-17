using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.EventArgs;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Hydrate_App.ViewModels;

public partial class HydrateViewModel : BaseViewModel
{
    readonly INotificationService _notificationService;
    readonly ILogger<HydrateViewModel> _logger;

    public int MinimumHydrateInterval => 5;
    public int MaximumHydrateInterval => 120;


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HydrateActiveLabel))]
    bool _notificationActive;
    public string HydrateActiveLabel => (NotificationActive) ? "Active" : "Inactive";

    [ObservableProperty]
    TimeOnly _doNotDisturbStartTime = new TimeOnly(20, 25);

    [ObservableProperty]
    TimeOnly _doNotDisturbEndTime = new TimeOnly(6, 00);

    int hydrateIntervalInMinutes;
    public int HydrateIntervalInMinutes
    {
        get => hydrateIntervalInMinutes;
        set => SetProperty(ref hydrateIntervalInMinutes, (int)(Math.Round(value / (double)MinimumHydrateInterval) * MinimumHydrateInterval));
    }

    public HydrateViewModel(INotificationService notificationService, ILogger<HydrateViewModel> logger) : base("Hydrate")
    {
        _notificationService = notificationService;
        _logger = logger;

        _logger.LogInformation("{} started", this);
    }

    [RelayCommand]
    public async Task ToggleHydrationTimerAsync()
    {
        if(IsBusy) return;
        else IsBusy = true;

        try
        {
            if (NotificationActive)
            {
                _notificationService.CancelAll();
                NotificationActive = false;
                return;
            }


            if (await _notificationService.AreNotificationsEnabled() is false)
            {
                await _notificationService.RequestNotificationPermission();

                if (await _notificationService.AreNotificationsEnabled() is false)
                    return;
            }

            var notification = new NotificationRequest()
            {
                Title = "Time to hydrate!",
                Description = "Your hydration timer has run out!",
                Silent = true,
                CategoryType = NotificationCategoryType.Alarm,

                Schedule =
            {
                 NotifyTime = DateTime.Now.AddSeconds(HydrateIntervalInMinutes),
                 RepeatType = NotificationRepeat.TimeInterval,
                 NotifyRepeatInterval = TimeSpan.FromSeconds(HydrateIntervalInMinutes)
            },
            };



            _notificationService.NotificationReceiving = (request) =>
            {
                var timeNow = TimeOnly.FromDateTime(DateTime.Now);

                var doNotDisturb = timeNow.IsBetween(DoNotDisturbStartTime, DoNotDisturbEndTime);

                return Task.FromResult(new NotificationEventReceivingArgs
                {
                    Handled = doNotDisturb, // doesnt pop up when true
                    Request = request
                });
            };


            await _notificationService.Show(notification);

            NotificationActive = true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while handling notifications");
        }
        finally
        {
            IsBusy = false;
        }


    }
}
