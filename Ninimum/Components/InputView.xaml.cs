using System.Windows.Input;

namespace Samokat.Components;

public partial class InputView : ContentView
{
    private bool _isPasswordHidden = true;
    private bool _isEntryFocused;

    public InputView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(
            nameof(TapCommand),
            typeof(ICommand),
            typeof(InputView));

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public static readonly BindableProperty CompletedCommandProperty =
        BindableProperty.Create(
            nameof(CompletedCommand),
            typeof(ICommand),
            typeof(InputView));

    public ICommand CompletedCommand
    {
        get => (ICommand)GetValue(CompletedCommandProperty);
        set => SetValue(CompletedCommandProperty, value);
    }

    public static readonly BindableProperty RightIconCommandProperty =
    BindableProperty.Create(
        nameof(RightIconCommand),
        typeof(ICommand),
        typeof(InputView));

    public ICommand RightIconCommand
    {
        get => (ICommand)GetValue(RightIconCommandProperty);
        set => SetValue(RightIconCommandProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(InputView),
            string.Empty,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(InputView),
            string.Empty,
            BindingMode.TwoWay,
            propertyChanged: OnTextChanged);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(InputView),
            string.Empty);

    public static readonly BindableProperty LeftIconProperty =
        BindableProperty.Create(
            nameof(LeftIcon),
            typeof(string),
            typeof(InputView),
            string.Empty,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty MiddleImageProperty =
        BindableProperty.Create(
            nameof(MiddleImage),
            typeof(string),
            typeof(InputView),
            string.Empty,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty RightIconProperty =
        BindableProperty.Create(
            nameof(RightIcon),
            typeof(string),
            typeof(InputView),
            string.Empty,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty RightIconVisibleProperty =
        BindableProperty.Create(
            nameof(RightIconVisible),
            typeof(bool),
            typeof(InputView),
            true,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty IsReadOnlyProperty =
        BindableProperty.Create(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(InputView),
            false);

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(
            nameof(IsPassword),
            typeof(bool),
            typeof(InputView),
            false,
            propertyChanged: OnPasswordPropertyChanged);

    public static readonly BindableProperty EnablePasswordToggleProperty =
        BindableProperty.Create(
            nameof(EnablePasswordToggle),
            typeof(bool),
            typeof(InputView),
            false,
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty PasswordShowIconProperty =
        BindableProperty.Create(
            nameof(PasswordShowIcon),
            typeof(string),
            typeof(InputView),
            "ic_eye.png",
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty PasswordHideIconProperty =
        BindableProperty.Create(
            nameof(PasswordHideIcon),
            typeof(string),
            typeof(InputView),
            "ic_eye_off.png",
            propertyChanged: OnVisualPropertyChanged);

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(
            nameof(Keyboard),
            typeof(Keyboard),
            typeof(InputView),
            Microsoft.Maui.Keyboard.Default);

    public static readonly BindableProperty ReturnTypeProperty =
        BindableProperty.Create(
            nameof(ReturnType),
            typeof(ReturnType),
            typeof(InputView),
            ReturnType.Done);

    public static readonly BindableProperty ClearButtonVisibilityProperty =
    BindableProperty.Create(
        nameof(ClearButtonVisibility),
        typeof(ClearButtonVisibility),
        typeof(InputView),
        ClearButtonVisibility.Never);

    public static readonly BindableProperty LeftIconSizeProperty =
    BindableProperty.Create(
        nameof(LeftIconSize),
        typeof(double),
        typeof(InputView),
        28.0);

    public double LeftIconSize
    {
        get => (double)GetValue(LeftIconSizeProperty);
        set => SetValue(LeftIconSizeProperty, value);
    }

    public static readonly BindableProperty RightIconSizeProperty =
        BindableProperty.Create(
            nameof(RightIconSize),
            typeof(double),
            typeof(InputView),
            24.0);

    public double RightIconSize
    {
        get => (double)GetValue(RightIconSizeProperty);
        set => SetValue(RightIconSizeProperty, value);
    }

    public ClearButtonVisibility ClearButtonVisibility
    {
        get => (ClearButtonVisibility)GetValue(ClearButtonVisibilityProperty);
        set => SetValue(ClearButtonVisibilityProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string LeftIcon
    {
        get => (string)GetValue(LeftIconProperty);
        set => SetValue(LeftIconProperty, value);
    }

    public string MiddleImage
    {
        get => (string)GetValue(MiddleImageProperty);
        set => SetValue(MiddleImageProperty, value);
    }

    public string RightIcon
    {
        get => (string)GetValue(RightIconProperty);
        set => SetValue(RightIconProperty, value);
    }

    public bool RightIconVisible
    {
        get => (bool)GetValue(RightIconVisibleProperty);
        set => SetValue(RightIconVisibleProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public bool EnablePasswordToggle
    {
        get => (bool)GetValue(EnablePasswordToggleProperty);
        set => SetValue(EnablePasswordToggleProperty, value);
    }

    public string PasswordShowIcon
    {
        get => (string)GetValue(PasswordShowIconProperty);
        set => SetValue(PasswordShowIconProperty, value);
    }

    public string PasswordHideIcon
    {
        get => (string)GetValue(PasswordHideIconProperty);
        set => SetValue(PasswordHideIconProperty, value);
    }

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public ReturnType ReturnType
    {
        get => (ReturnType)GetValue(ReturnTypeProperty);
        set => SetValue(ReturnTypeProperty, value);
    }

    public bool HasLeftIcon => !string.IsNullOrWhiteSpace(LeftIcon);
    public bool HasMiddleImage => !string.IsNullOrWhiteSpace(MiddleImage);

    public bool HasRightIcon
    {
        get
        {
            if (!RightIconVisible)
                return false;

            if (EnablePasswordToggle && IsPassword)
                return true;

            return !string.IsNullOrWhiteSpace(RightIcon);
        }
    }

    public bool HasTitle => !string.IsNullOrWhiteSpace(Title);

    public bool IsTitleVisible =>
        !string.IsNullOrWhiteSpace(Title) &&
        string.IsNullOrWhiteSpace(Text) &&
        !_isEntryFocused;

    public bool CurrentIsPassword => IsPassword && _isPasswordHidden;

    public string CurrentRightIcon
    {
        get
        {
            if (EnablePasswordToggle && IsPassword)
                return _isPasswordHidden ? PasswordHideIcon : PasswordShowIcon;

            return RightIcon;
        }
    }

    public void FocusEntry()
    {
        if (!IsReadOnly)
            PART_Entry.Focus();
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (InputView)bindable;
        view.OnPropertyChanged(nameof(IsTitleVisible));
    }

    private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (InputView)bindable;
        view.RefreshVisualState();
    }

    private static void OnPasswordPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (InputView)bindable;
        view._isPasswordHidden = true;
        view.RefreshVisualState();
    }

    private void RefreshVisualState()
    {
        OnPropertyChanged(nameof(HasLeftIcon));
        OnPropertyChanged(nameof(HasMiddleImage));
        OnPropertyChanged(nameof(HasRightIcon));
        OnPropertyChanged(nameof(HasTitle));
        OnPropertyChanged(nameof(IsTitleVisible));
        OnPropertyChanged(nameof(CurrentIsPassword));
        OnPropertyChanged(nameof(CurrentRightIcon));
    }

    private void OnBorderTapped(object sender, TappedEventArgs e)
    {
        if (!IsReadOnly)
            PART_Entry.Focus();

        if (TapCommand?.CanExecute(null) == true)
            TapCommand.Execute(null);
    }

    private async void OnRightIconTapped(object sender, TappedEventArgs e)
    {
        if (sender is Image img)
        {
            await img.ScaleTo(0.96, 100, Easing.CubicOut);
            await img.ScaleTo(1.0, 100, Easing.CubicIn);
        }

        // 1. Internal behavior (password toggle)
        if (EnablePasswordToggle && IsPassword)
        {
            _isPasswordHidden = !_isPasswordHidden;
            OnPropertyChanged(nameof(CurrentIsPassword));
            OnPropertyChanged(nameof(CurrentRightIcon));

            if (!IsReadOnly)
                PART_Entry.Focus();
        }

        // 2. Specific command for right icon
        if (RightIconCommand?.CanExecute(null) == true)
        {
            RightIconCommand.Execute(null);
            return;
        }

        // 3. Fallback (optional but useful)
        if (TapCommand?.CanExecute(null) == true)
        {
            TapCommand.Execute(null);
        }
    }

    private void OnEntryFocused(object sender, FocusEventArgs e)
    {
        _isEntryFocused = true;
        OnPropertyChanged(nameof(IsTitleVisible));
    }

    private void OnEntryUnfocused(object sender, FocusEventArgs e)
    {
        _isEntryFocused = false;
        OnPropertyChanged(nameof(IsTitleVisible));
    }

    private void OnEntryCompleted(object sender, EventArgs e)
    {
        if (CompletedCommand?.CanExecute(null) == true)
            CompletedCommand.Execute(null);
    }
}