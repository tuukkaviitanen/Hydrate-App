using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using Plugin.LocalNotification.EventArgs;

namespace Hydrate_App.Services;
public class HydrationNotificationService
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<HydrationNotificationService> _logger;

    public HydrationNotificationService(INotificationService notificationService, ILogger<HydrationNotificationService> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<bool> SetNotification(int hydrateInterval, bool isDoNotDisturbEnabled, TimeSpan doNotDisturbStartTime, TimeSpan doNotDisturbEndTime)
    {
        try
        {
            _notificationService.CancelAll();

            while (await _notificationService.AreNotificationsEnabled() is false)
            {
                await _notificationService.RequestNotificationPermission();
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
                    NotifyTime = DateTime.Now.AddSeconds(hydrateInterval),
                    NotifyRepeatInterval = TimeSpan.FromSeconds(hydrateInterval),
#else
                    NotifyTime = DateTime.Now.AddMinutes(hydrateInterval),
                    NotifyRepeatInterval = TimeSpan.FromMinutes(hydrateInterval),
#endif
                    RepeatType = NotificationRepeat.TimeInterval,
                },
            };

            _notificationService.NotificationReceiving = (request) =>
            {
                var timeNow = TimeOnly.FromDateTime(DateTime.Now);
                var startTime = TimeOnly.FromTimeSpan(doNotDisturbStartTime);
                var endTime = TimeOnly.FromTimeSpan(doNotDisturbEndTime);

                var doNotDisturb = isDoNotDisturbEnabled && timeNow.IsBetween(startTime, endTime);

                return Task.FromResult(new NotificationEventReceivingArgs
                {
                    Handled = doNotDisturb, // doesn't pop up when true
                    Request = request
                });
            };

            await _notificationService.Show(notification);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Setting notification failed");
            return false;
        }
    }

    public void CancelNotifications()
    {
        _notificationService.CancelAll();
        _notificationService.ClearAll();
    }
}
