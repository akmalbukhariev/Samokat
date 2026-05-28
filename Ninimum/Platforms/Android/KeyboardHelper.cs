
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
        var inputMethodManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);
        var token = activity.CurrentFocus?.WindowToken;
        inputMethodManager?.HideSoftInputFromWindow(token, HideSoftInputFlags.None);
    }
}