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
      BindableProperty.Create(nameof(ShowBack), typeof(bool), typeof(PageHeaderView), true, propertyChanged: OnShowBackChanged);
    
    public bool ShowBack
    {
        get => (bool)GetValue(ShowBackProperty);
        set => SetValue(ShowBackProperty, value);
    }
    
    private static void OnShowBackChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (PageHeaderView)bindable;
        control.imBack.IsVisible = (bool)newValue;
    }

    private async void Back_Tapped(object sender, TappedEventArgs e)
    {
        await AnimateElementScaleDown(imBack);

        if (BackCommand?.CanExecute(null) == true)
            BackCommand.Execute(null);
    }

    protected Task AnimateElementScaleDown(VisualElement element)
        {
            return Task.Run(async () =>
            {
                await element.ScaleTo(0.9, 100, Easing.CubicOut);
                await element.ScaleTo(1.0, 100, Easing.CubicIn);
            });
        }
}