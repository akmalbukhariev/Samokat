using Ninimum.Resources.Languages;
using System.Windows.Input;
using Ninimum.Services;

namespace Ninimum.Components;

public partial class SmsCodePopup : ContentView
{
    private string _smsCode = string.Empty;
    private IDispatcherTimer? _timer;
    private int _secondsLeft = 45;
    private bool _isExpired;

    public ICommand ConfirmTapCommand { get; }

    private readonly IKeyboardHelper keyboardHelper;

    public SmsCodePopup()
    {
        InitializeComponent();

        keyboardHelper = AppService.Get<IKeyboardHelper>();
        ConfirmTapCommand = new Command(OnConfirmTapped);

        UpdateOtpUI();
        BindingContext = this;
    }

    public static readonly BindableProperty ConfirmCommandProperty =
        BindableProperty.Create(
            nameof(ConfirmCommand),
            typeof(ICommand),
            typeof(SmsCodePopup));

    public ICommand ConfirmCommand
    {
        get => (ICommand)GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }

    public static readonly BindableProperty SmsCodeProperty =
        BindableProperty.Create(
            nameof(SmsCode),
            typeof(string),
            typeof(SmsCodePopup),
            string.Empty,
            BindingMode.TwoWay);

    public string SmsCode
    {
        get => (string)GetValue(SmsCodeProperty);
        set => SetValue(SmsCodeProperty, value);
    }

    public static readonly BindableProperty ResendCommandProperty =
        BindableProperty.Create(
            nameof(ResendCommand),
            typeof(ICommand),
            typeof(SmsCodePopup));

    public ICommand ResendCommand
    {
        get => (ICommand)GetValue(ResendCommandProperty);
        set => SetValue(ResendCommandProperty, value);
    }

    public void Show()
    {
        IsVisible = true;
        Reset();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(100);

            if (IsVisible && !_isExpired)
                HiddenOtpEntry.Focus();
        });
    }

    public void Hide()
    {
        _timer?.Stop();
        keyboardHelper.HideKeyboard();
        HiddenOtpEntry.Unfocus();
        IsVisible = false;
    }

    public void Reset()
    {
        _isExpired = false;
        _secondsLeft = 45;
        _smsCode = string.Empty;

        SmsCode = string.Empty;
        HiddenOtpEntry.Text = string.Empty;

        lblTimer.Text = FormatTime(_secondsLeft);
        lblTimer.TextColor = Color.FromArgb("#96979B");
        lblTimer.InputTransparent = true;
        lblTimer.Opacity = 0.6;

        UpdateOtpUI();
        RestartTimer();
    }

    private void OnBackgroundTapped(object sender, TappedEventArgs e)
    {
        // Keep background taps from closing the popup accidentally.
        // The explicit X button is used to close it.
    }

    private async void CloseButton_Tapped(object sender, TappedEventArgs e)
    {
        await AnimateElementScaleDown(CloseButton);
        Hide();
    }

    private void OnConfirmTapped()
    {
        if (_isExpired || _secondsLeft <= 0 || _smsCode.Length != 4)
            return;

        if (ConfirmCommand?.CanExecute(_smsCode) == true)
            ConfirmCommand.Execute(_smsCode);
    }

    private void OtpArea_Tapped(object sender, TappedEventArgs e)
    {
        if (_isExpired)
            return;

        HiddenOtpEntry.Focus();
    }

    private void HiddenOtpEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isExpired)
        {
            if (!string.IsNullOrEmpty(HiddenOtpEntry.Text))
                HiddenOtpEntry.Text = string.Empty;

            return;
        }

        var newText = e.NewTextValue ?? string.Empty;
        newText = new string(newText.Where(char.IsDigit).ToArray());

        if (newText.Length > 4)
            newText = newText[..4];

        if (HiddenOtpEntry.Text != newText)
        {
            HiddenOtpEntry.Text = newText;
            return;
        }

        _smsCode = newText;
        SmsCode = newText;
        UpdateOtpUI();

        if (_smsCode.Length == 4)
            HiddenOtpEntry.Unfocus();
    }

    private async void HiddenOtpEntry_Focused(object sender, FocusEventArgs e)
    {
        if (!_isExpired)
            await PopupCard.TranslateTo(0, -90, 180, Easing.CubicOut);
    }

    private async void HiddenOtpEntry_Unfocused(object sender, FocusEventArgs e)
    {
        await Task.Delay(80);

        if (!HiddenOtpEntry.IsFocused)
            await PopupCard.TranslateTo(0, 0, 180, Easing.CubicOut);
    }

    private void HiddenOtpEntry_Completed(object sender, EventArgs e)
    {
        if (_isExpired)
            return;

        if (_smsCode.Length < 4)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(50);

                if (IsVisible && !_isExpired)
                    HiddenOtpEntry.Focus();
            });
        }
    }

    private void UpdateOtpUI()
    {
        Digit1Label.Text = _smsCode.Length > 0 ? _smsCode[0].ToString() : string.Empty;
        Digit2Label.Text = _smsCode.Length > 1 ? _smsCode[1].ToString() : string.Empty;
        Digit3Label.Text = _smsCode.Length > 2 ? _smsCode[2].ToString() : string.Empty;
        Digit4Label.Text = _smsCode.Length > 3 ? _smsCode[3].ToString() : string.Empty;

        SetBoxState(Box1, Cursor1, !_isExpired && _smsCode.Length == 0);
        SetBoxState(Box2, Cursor2, !_isExpired && _smsCode.Length == 1);
        SetBoxState(Box3, Cursor3, !_isExpired && _smsCode.Length == 2);
        SetBoxState(Box4, Cursor4, !_isExpired && _smsCode.Length == 3);

        if (_smsCode.Length >= 4 || _isExpired)
        {
            Cursor1.IsVisible = false;
            Cursor2.IsVisible = false;
            Cursor3.IsVisible = false;
            Cursor4.IsVisible = false;
        }

        bool canConfirm = !_isExpired && _secondsLeft > 0 && _smsCode.Length == 4;
        btnConfirmCode.IsEnabled = canConfirm;

        btnConfirmCode.ButtonBackgroundColor =
            canConfirm
                ? Color.FromArgb("#FD473C")
                : Color.FromArgb("#D9D9D9");

        btnConfirmCode.ButtonTextColor =
            canConfirm
                ? Colors.White
                : Color.FromArgb("#9E9E9E");

        if (canConfirm)
            keyboardHelper.HideKeyboard();
    }

    private void SetBoxState(Border box, BoxView cursor, bool isActive)
    {
        if (isActive)
        {
            box.BackgroundColor = Colors.White;
            box.Stroke = Color.FromArgb("#DADADA");
            box.StrokeThickness = 1;
            cursor.IsVisible = true;
        }
        else
        {
            box.BackgroundColor = Color.FromArgb("#F7F8FB");
            box.Stroke = Colors.Transparent;
            box.StrokeThickness = 0;
            cursor.IsVisible = false;
        }
    }

    private void StartTimer()
    {
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void RestartTimer()
    {
        if (_timer == null)
        {
            StartTimer();
            return;
        }

        _timer.Stop();
        _timer.Start();
    }

    private async void LblTimer_Tapped(object sender, TappedEventArgs e)
    {
        if (!_isExpired)
            return;

        await AnimateElementScaleDown(lblTimer);

        if (ResendCommand?.CanExecute(null) != true)
            return;

        ResendCommand.Execute(null);
        Reset();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_isExpired)
        {
            _timer?.Stop();
            return;
        }

        _secondsLeft--;

        if (_secondsLeft <= 0)
        {
            ExpireCode();
            return;
        }

        lblTimer.Text = FormatTime(_secondsLeft);
    }

    private void ExpireCode()
    {
        _secondsLeft = 0;
        _isExpired = true;
        _timer?.Stop();

        _smsCode = string.Empty;
        SmsCode = string.Empty;
        HiddenOtpEntry.Text = string.Empty;
        HiddenOtpEntry.Unfocus();

        lblTimer.Text = AppResource.Resend;
        lblTimer.TextColor = Color.FromArgb("#FD473C");
        lblTimer.InputTransparent = false;
        lblTimer.Opacity = 1;

        UpdateOtpUI();
    }

    private static string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes}:{seconds:D2}";
    }

    protected async Task AnimateElementScaleDown(VisualElement element)
    {
        await element.ScaleTo(0.9, 100, Easing.CubicOut);
        await element.ScaleTo(1.0, 100, Easing.CubicIn);
    }
}
