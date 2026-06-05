using Microsoft.Maui.Devices;

namespace Ninimum.Services;

public static class AppVibrationService
{
    public static void Click()
    {
        TryVibrate(50);
    }

    public static void Like()
    {
        TryVibrate(80);
    }

    public static void Success()
    {
        TryVibrate(120);
    }

    public static void Warning()
    {
        TryVibrate(200);
    }

    public static void TryVibrate(int milliseconds)
    {
        try
        {
            Vibration.Default.Vibrate(
                TimeSpan.FromMilliseconds(milliseconds));
        }
        catch
        {
            // Device doesn't support vibration
        }
    }
}