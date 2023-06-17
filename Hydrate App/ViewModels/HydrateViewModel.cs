using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.EventArgs;

namespace Hydrate_App.ViewModels;

public partial class HydrateViewModel : BaseViewModel
{
    readonly INotificationService _notificationService;
    readonly ILogger<HydrateViewModel> _logger;

    public int MinimumHydrateInterval => 5;
    public int MaximumHydrateInterval => 120;

    [ObservableProperty]
    private bool _isHydrationTimerEnabled;

    [ObservableProperty]
    private bool _isDoNotDisturbEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HydrateActiveLabel))]
    bool _notificationActive;
    public string HydrateActiveLabel => (NotificationActive) ? "Active" : "Inactive";

    [ObservableProperty]
    TimeSpan _doNotDisturbStartTime;

    [ObservableProperty]
    TimeSpan _doNotDisturbEndTime;


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
                Silent = false,
                Android = new AndroidOptions()
                {
                    IconSmallName = new AndroidIcon("droplet_solid"),
                },
                CategoryType = NotificationCategoryType.Alarm,

                Schedule =
                {
#if DEBUG
                    NotifyTime = DateTime.Now.AddSeconds(HydrateIntervalInMinutes),
                    NotifyRepeatInterval = TimeSpan.FromSeconds(HydrateIntervalInMinutes),
#else
                    NotifyTime = DateTime.Now.AddMinutes(HydrateIntervalInMinutes),
                    NotifyRepeatInterval = TimeSpan.FromMinutes(HydrateIntervalInMinutes),
#endif
                    RepeatType = NotificationRepeat.TimeInterval,
                },
            };

            _notificationService.NotificationReceiving = (request) =>
            {
                var timeNow = TimeOnly.FromDateTime(DateTime.Now);
                var startTime = TimeOnly.FromTimeSpan(DoNotDisturbStartTime);
                var endTime = TimeOnly.FromTimeSpan(DoNotDisturbEndTime);

                var doNotDisturb = IsDoNotDisturbEnabled && timeNow.IsBetween(startTime, endTime);

                return Task.FromResult(new NotificationEventReceivingArgs
                {
                    Handled = doNotDisturb, // doesn't pop up when true
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
