namespace Samokat.Components;

public partial class QuestionBannerView : ContentView
{
    public QuestionBannerView()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty BannerTextProperty =
        BindableProperty.Create(
            nameof(BannerText),
            typeof(string),
            typeof(QuestionBannerView),
            default(string));

    public string BannerText
    {
        get => (string)GetValue(BannerTextProperty);
        set => SetValue(BannerTextProperty, value);
    }
}