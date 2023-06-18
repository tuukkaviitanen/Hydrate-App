using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hydrate_App.Services;
using Microsoft.Extensions.Logging;

namespace Hydrate_App.ViewModels;

/// <summary>
/// ViewModel for Hydration notifier
/// </summary>
public partial class HydrateViewModel : BaseViewModel
{
    readonly HydrationNotificationService _notificationService;
    readonly ILogger<HydrateViewModel> _logger;

    // Only non-static properties can be read from View
    public int MinimumHydrateInterval => 5;
    public int MaximumHydrateInterval => 120;

    /// <summary>
    /// True if Hydration notifications are enabled. 
    /// When changed/set, enables or disables notifications
    /// </summary>
    private bool _isHydrationTimerEnabled;
    /// <inheritdoc cref="_isHydrationTimerEnabled"/>
    public bool IsHydrationTimerEnabled
    {
        get => _isHydrationTimerEnabled;
        set
        {
            if(value == _isHydrationTimerEnabled) return;

            if (value is true)
            {
                SetNotificationCommand.Execute(null);
            }
            else
            {
                CancelNotification();
            }

            SetProperty(ref _isHydrationTimerEnabled, value);
        }
    }

    /// <summary>
    /// True if Do-Not-Disturb is enabled. 
    /// Also notifies <see cref="IsUnsavedChanges"/> that there might be unsaved changes
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUnsavedChanges))]
    private bool _isDoNotDisturbEnabled = PreferenceService.IsDoNotDisturbEnabled;

    /// <summary>
    /// Starting time of Do-Not-Disturb.
    /// Also notifies <see cref="IsUnsavedChanges"/> that there might be unsaved changes
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUnsavedChanges))]
    TimeSpan _doNotDisturbStartTime = PreferenceService.DoNotDisturbStartTime;

    /// <summary>
    /// Ending time of Do-Not-Disturb.
    /// Also notifies <see cref="IsUnsavedChanges"/> that there might be unsaved changes
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUnsavedChanges))]
    TimeSpan _doNotDisturbEndTime = PreferenceService.DoNotDisturbEndTime;

    /// <summary>
    /// Hydration notification interval in full minutes.
    /// Also notifies <see cref="IsUnsavedChanges"/> that there might be unsaved changes
    /// </summary>
    int _hydrateIntervalInMinutes = PreferenceService.HydrateIntervalInMinutes;
    /// <inheritdoc cref="_hydrateIntervalInMinutes"/>
    public int HydrateIntervalInMinutes
    {
        get => _hydrateIntervalInMinutes;
        set
        {
            var result = (int)(Math.Round(value / (double)MinimumHydrateInterval) * MinimumHydrateInterval);
            SetProperty(ref _hydrateIntervalInMinutes, result);
            OnPropertyChanged(nameof(IsUnsavedChanges));
        }
    }

    /// <summary>
    /// True if there are any differences between saved preferences and current ViewModel values
    /// </summary>
    public bool IsUnsavedChanges => HydrateIntervalInMinutes != PreferenceService.HydrateIntervalInMinutes ||
        DoNotDisturbStartTime != PreferenceService.DoNotDisturbStartTime ||
        DoNotDisturbEndTime != PreferenceService.DoNotDisturbEndTime ||
        IsDoNotDisturbEnabled != PreferenceService.IsDoNotDisturbEnabled;

    [ObservableProperty]
    private DateTime? _upcomingNotification;

    public HydrateViewModel(HydrationNotificationService notificationService, ILogger<HydrateViewModel> logger) : base("Hydrate")
    {
        _notificationService = notificationService;
        _logger = logger;

        // Runs async task to check if Hydration notifications are enabled in NotificationService.
        // Uses private field to not call property setter and reset notifications
        Task.Run(async () =>
        {
            _isHydrationTimerEnabled = await _notificationService.IsNotificationActive();
            OnPropertyChanged(nameof(IsHydrationTimerEnabled));

            await SubscribeToUpcomingNotifications();
        }
        );

        _logger.LogInformation("{} started", this);

    }

    /// <summary>
    /// Sets current ViewModel values to Preferences. 
    /// Sets a new reoccurring notification through <see cref="HydrationNotificationService"/>.
    /// </summary>
    [RelayCommand]
    public async Task SetNotification()
    {
        if (IsBusy) return;
        else IsBusy = true;

        PreferenceService.IsDoNotDisturbEnabled = IsDoNotDisturbEnabled;
        PreferenceService.DoNotDisturbStartTime = DoNotDisturbStartTime;
        PreferenceService.DoNotDisturbEndTime = DoNotDisturbEndTime;
        PreferenceService.HydrateIntervalInMinutes = HydrateIntervalInMinutes;

        await _notificationService.SetNotification(
            HydrateIntervalInMinutes, 
            IsDoNotDisturbEnabled, 
            DoNotDisturbStartTime, 
            DoNotDisturbEndTime);

        await SubscribeToUpcomingNotifications();

        OnPropertyChanged(nameof(IsUnsavedChanges));
        IsBusy = false;
    }

    /// <summary>
    /// Cancels reoccurring notifications through <see cref="HydrationNotificationService"/>.
    /// </summary>
    public void CancelNotification()
    {
        _notificationService.CancelNotifications();

        UpcomingNotification = null;
    }

    /// <summary>
    /// Binds upcoming notifications to <see cref="UpcomingNotification"/>
    /// </summary>
    public async Task SubscribeToUpcomingNotifications()
    {
        await _notificationService.SubscribeToUpcomingNotifications(
            (notification) => UpcomingNotification = notification
            );
        
    }
}
