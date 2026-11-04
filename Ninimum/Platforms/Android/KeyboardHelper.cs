
using Android.Content;
using Android.Views.InputMethods;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Ninimum.Services;

[assembly: Dependency(typeof(KeyboardHelper))]
public class KeyboardHelper : IKeyboardHelper
{
    public void HideKeyboard()
    {
        var activity = Platform.CurrentActivity;
        if (activity is null) return;
        var inputMethodManager = (InputMethodManager?)activity.GetSystemService(Context.InputMethodService);
        // Without a focused field, use the window itself so the keyboard still closes.
        var token = activity.CurrentFocus?.WindowToken ?? activity.Window?.DecorView?.WindowToken;
        if (token != null) inputMethodManager?.HideSoftInputFromWindow(token, HideSoftInputFlags.None);
        activity.CurrentFocus?.ClearFocus();
    }
}