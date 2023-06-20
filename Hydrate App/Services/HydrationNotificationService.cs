using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using Plugin.LocalNotification.AndroidOption;
using System.Diagnostics;

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

    public async Task<bool> CreateNotifications(int hydrateInterval, bool isDoNotDisturbEnabled, TimeSpan doNotDisturbStartTime, TimeSpan doNotDisturbEndTime)
    {
        try
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            _notificationService.CancelAll();

            // Checks Notification permissions
            while (await _notificationService.AreNotificationsEnabled() is false)
            {
                await _notificationService.RequestNotificationPermission();
            }

            TimeSpan notificationInterval = TimeSpan.FromMinutes(hydrateInterval);
            DateTime startingTime = DateTime.Now.Add(notificationInterval);
            DateTime endTime = startingTime.AddDays(1);

            var doNotDisturbStart = TimeOnly.FromTimeSpan(doNotDisturbStartTime);
            var doNotDisturbEnd = TimeOnly.FromTimeSpan(doNotDisturbEndTime);

            var tasks = new List<Task>(); // Creates a list of tasks to await at the same time instead of sequentially

            for (DateTime notificationTime = startingTime; notificationTime < endTime; notificationTime = notificationTime.Add(notificationInterval))
            {
                if (isDoNotDisturbEnabled && TimeOnly.FromDateTime(notificationTime).IsBetween(doNotDisturbStart, doNotDisturbEnd))
                {
                    continue;
                }

                tasks.Add(CreateNotification(tasks.Count, notificationTime));
            }

            await Task.WhenAll(tasks); // runs all tasks parallel // might improve performance, might not // just an interesting test

            stopWatch.Stop();
            _logger.LogInformation("Creating notifications took {0} milliseconds, {1} notifications created", stopWatch.Elapsed.Microseconds, tasks.Count);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Setting notification failed");
            return false;
        }
    }

    private async Task CreateNotification(int notificationId, DateTime notificationTime)
    {
        var notification = new NotificationRequest()
        {
            NotificationId = notificationId,
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
                NotifyTime = notificationTime,
                RepeatType = NotificationRepeat.Daily
            },
        };

        await _notificationService.Show(notification);
    }

    public void CancelNotifications()
    {
        _notificationService.CancelAll();
        _notificationService.ClearAll();
    }

    /// <summary>
    /// Checks if there are any upcoming notifications
    /// </summary>
    /// <returns></returns>
    public async Task<bool> AreNotificationsActive()
    {
        var notificationList = await _notificationService.GetPendingNotificationList();

        return notificationList.Any(x => x.Schedule?.NotifyTime >= DateTime.Now);
    }

    /// <summary>
    /// Subscribes to NotificationReceived event and runs given callback function when fired.
    /// Also runs callback immediately.
    /// </summary>
    /// <param name="callback">Action to run when notification is received. Next notification datetime given as a parameter</param>
    public async Task SubscribeToUpcomingNotifications(Action<DateTime?> callback)
    {
        callback(await GetUpcomingNotificationTime());

        _notificationService.NotificationReceived += async (_) =>
        {
            callback(await GetUpcomingNotificationTime());
        };

    }

    /// <summary>
    /// Gets next pending notification
    /// </summary>
    /// <returns>Next pending notification OR null if no notifications pending</returns>
    private async Task<DateTime?> GetUpcomingNotificationTime()
    {
        var notificationList = await _notificationService.GetPendingNotificationList();
        NotificationRequest? notification = notificationList.OrderBy(x => x.Schedule.NotifyTime)
            .FirstOrDefault(x => x.Schedule.NotifyTime.HasValue && x.Schedule.NotifyTime.Value > DateTime.Now);

        return notification?.Schedule.NotifyTime;
    }


}
