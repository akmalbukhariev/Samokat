using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Samokat;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		Setting();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}

	private void Setting()
        {
#if ANDROID
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
            });

            Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping(nameof(Picker), (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
            });

            Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping(nameof(DatePicker), (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
            });

            Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping(nameof(TimePicker), (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToPlatform());
            });

            EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
                if (handler.PlatformView is Android.Widget.EditText editText)
                {
                    editText.Background = null; // Removes underline
                }
            });
#endif

#if IOS
            
            EntryHandler.Mapper.AppendToMapping(nameof(Entry), (handler, view) =>
            {
                handler.PlatformView.BorderStyle = UITextBorderStyle.None;
                handler.PlatformView.Layer.BorderWidth = 0;
                handler.PlatformView.BackgroundColor = UIColor.Clear;
            });

            PickerHandler.Mapper.AppendToMapping(nameof(Picker), (handler, view) =>
            {
                handler.PlatformView.BorderStyle = UITextBorderStyle.None;
                handler.PlatformView.Layer.BorderWidth = 0;
                handler.PlatformView.BackgroundColor = UIColor.Clear;
            });

            DatePickerHandler.Mapper.AppendToMapping(nameof(DatePicker), (handler, view) =>
            {
                handler.PlatformView.BorderStyle = UITextBorderStyle.None;
                handler.PlatformView.Layer.BorderWidth = 0;
                handler.PlatformView.BackgroundColor = UIColor.Clear;
            });

            TimePickerHandler.Mapper.AppendToMapping(nameof(TimePicker), (handler, view) =>
            {
                handler.PlatformView.BorderStyle = UITextBorderStyle.None;
                handler.PlatformView.Layer.BorderWidth = 0;
                handler.PlatformView.BackgroundColor = UIColor.Clear;
            });

            EditorHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
            {
                // Editor is UITextView on iOS
                var textView = handler.PlatformView;
                textView.Layer.BorderWidth = 0;
                textView.BackgroundColor = UIColor.Clear;
            });
#endif
        }
}