
namespace Hydrate_App.Services;
/// <summary>
/// Uses <see cref="Preferences"/> to save and load values even after application is closed.
/// </summary>
public static class PreferenceService
{
    public static int HydrateIntervalInMinutes
    {
        get => Preferences.Get("HydrateIntervalInMinutes", default(int));
        set => Preferences.Set("HydrateIntervalInMinutes", value);
    }
    public static bool IsDoNotDisturbEnabled
    {
        get => Preferences.Get("IsDoNotDisturbEnabled", default(bool));
        set => Preferences.Set("IsDoNotDisturbEnabled", value);
    }
    public static TimeSpan DoNotDisturbStartTime
    {
        get => TimeSpan.FromTicks(Preferences.Get("DoNotDisturbStartTime", TimeSpan.Zero.Ticks));
        set => Preferences.Set("DoNotDisturbStartTime", value.Ticks);
    }
    public static TimeSpan DoNotDisturbEndTime
    {
        get => TimeSpan.FromTicks(Preferences.Get("DoNotDisturbEndTime", TimeSpan.Zero.Ticks));
        set => Preferences.Set("DoNotDisturbEndTime", value.Ticks);
    }
}
