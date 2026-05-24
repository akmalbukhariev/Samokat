using System.Windows.Input;

namespace Samokat.Components;

public partial class PageHeaderView : ContentView
{
    public PageHeaderView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(PageHeaderView),
            default(string));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty BackCommandProperty =
        BindableProperty.Create(
            nameof(BackCommand),
            typeof(ICommand),
            typeof(PageHeaderView));

    public ICommand BackCommand
    {
        get => (ICommand)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public static readonly BindableProperty ShowBackProperty =
        BindableProperty.Create(
            nameof(ShowBack),
            typeof(bool),
            typeof(PageHeaderView),
            true);

    public bool ShowBack
    {
        get => (bool)GetValue(ShowBackProperty);
        set => SetValue(ShowBackProperty, value);
    }

    public static readonly BindableProperty RightImageProperty =
        BindableProperty.Create(
            nameof(RightImage),
            typeof(ImageSource),
            typeof(PageHeaderView),
            default(ImageSource));

    public ImageSource RightImage
    {
        get => (ImageSource)GetValue(RightImageProperty);
        set => SetValue(RightImageProperty, value);
    }

    public static readonly BindableProperty ShowRightIconProperty =
        BindableProperty.Create(
            nameof(ShowRightIcon),
            typeof(bool),
            typeof(PageHeaderView),
            false);

    public bool ShowRightIcon
    {
        get => (bool)GetValue(ShowRightIconProperty);
        set => SetValue(ShowRightIconProperty, value);
    }

    public static readonly BindableProperty RightImageCommandProperty =
        BindableProperty.Create(
            nameof(RightImageCommand),
            typeof(ICommand),
            typeof(PageHeaderView));

    public ICommand RightImageCommand
    {
        get => (ICommand)GetValue(RightImageCommandProperty);
        set => SetValue(RightImageCommandProperty, value);
    }

    private async void Back_Tapped(object sender, TappedEventArgs e)
    {
        await AnimateElementScaleDown(imBack);

        if (BackCommand?.CanExecute(null) == true)
            BackCommand.Execute(null);

        await AppNavigatorService.NavigateTo("..");
    }

    private async void RightImage_Tapped(object sender, TappedEventArgs e)
    {
        await AnimateElementScaleDown(imRightImage);

        if (RightImageCommand?.CanExecute(null) == true)
            RightImageCommand.Execute(null);
    }

    protected Task AnimateElementScaleDown(VisualElement element)
    {
        return Task.Run(async () =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await element.ScaleTo(0.9, 100, Easing.CubicOut);
                await element.ScaleTo(1.0, 100, Easing.CubicIn);
            });
        });
    }
}