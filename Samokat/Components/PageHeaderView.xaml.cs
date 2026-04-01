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
            typeof(PageHeaderView),
            default(ICommand));

    public ICommand BackCommand
    {
        get => (ICommand)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public static readonly BindableProperty BackIconProperty =
        BindableProperty.Create(
            nameof(BackIcon),
            typeof(string),
            typeof(PageHeaderView),
            "back.png");

    public string BackIcon
    {
        get => (string)GetValue(BackIconProperty);
        set => SetValue(BackIconProperty, value);
    }
}