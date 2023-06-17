using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Hydrate_App.Services;
using Microsoft.Extensions.Logging;

namespace Hydrate_App.ViewModels;

public partial class HydrateViewModel : BaseViewModel
{
    readonly HydrationNotificationService _notificationService;
    readonly ILogger<HydrateViewModel> _logger;

    public int MinimumHydrateInterval => 5;
    public int MaximumHydrateInterval => 120;

    private bool _isHydrationTimerEnabled;
    public bool IsHydrationTimerEnabled
    {
        get => _isHydrationTimerEnabled;
        set
        {
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

    private bool _isDoNotDisturbEnabled = PreferenceService.IsDoNotDisturbEnabled;
    public bool IsDoNotDisturbEnabled
    {
        get => _isDoNotDisturbEnabled;
        set
        {
            SetProperty(ref _isDoNotDisturbEnabled, value);
            OnPropertyChanged(nameof(IsUnsavedChanges));
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HydrateActiveLabel))]
    bool _notificationActive;
    public string HydrateActiveLabel => (NotificationActive) ? "Active" : "Inactive";

    TimeSpan _doNotDisturbStartTime = PreferenceService.DoNotDisturbStartTime;

    public TimeSpan DoNotDisturbStartTime
    {
        get => _doNotDisturbStartTime;
        set
        {
            SetProperty(ref  _doNotDisturbStartTime, value);
            OnPropertyChanged(nameof(IsUnsavedChanges));
        }
    }

    TimeSpan _doNotDisturbEndTime = PreferenceService.DoNotDisturbEndTime;
    public TimeSpan DoNotDisturbEndTime
    {
        get => _doNotDisturbEndTime;
        set
        {
            SetProperty(ref _doNotDisturbEndTime, value);
            OnPropertyChanged(nameof(IsUnsavedChanges));
        }
    }

    int _hydrateIntervalInMinutes = PreferenceService.HydrateIntervalInMinutes;
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

    public bool IsUnsavedChanges => _hydrateIntervalInMinutes != PreferenceService.HydrateIntervalInMinutes ||
        _doNotDisturbStartTime != PreferenceService.DoNotDisturbStartTime ||
        _doNotDisturbEndTime != PreferenceService.DoNotDisturbEndTime ||
        _isDoNotDisturbEnabled != PreferenceService.IsDoNotDisturbEnabled;

    public HydrateViewModel(HydrationNotificationService notificationService, ILogger<HydrateViewModel> logger) : base("Hydrate")
    {
        _notificationService = notificationService;
        _logger = logger;

        Task.Run(async () => IsHydrationTimerEnabled = await _notificationService.IsNotificationActive());

        _logger.LogInformation("{} started", this);

    }

    [RelayCommand]
    public async Task SetNotification()
    {
        if (IsBusy) return;
        else IsBusy = true;

        PreferenceService.IsDoNotDisturbEnabled = IsDoNotDisturbEnabled;
        PreferenceService.DoNotDisturbStartTime = DoNotDisturbStartTime;
        PreferenceService.DoNotDisturbEndTime = DoNotDisturbEndTime;
        PreferenceService.HydrateIntervalInMinutes = HydrateIntervalInMinutes;

        NotificationActive = await _notificationService.SetNotification(
            HydrateIntervalInMinutes, 
            IsDoNotDisturbEnabled, 
            DoNotDisturbStartTime, 
            DoNotDisturbEndTime);

        OnPropertyChanged(nameof(IsUnsavedChanges));
        IsBusy = false;
    }

    public void CancelNotification()
    {
        _notificationService.CancelNotifications();
    }
}
