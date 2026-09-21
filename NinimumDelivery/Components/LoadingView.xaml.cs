namespace NinimumDelivery.Components;
public partial class LoadingView : ContentView
{
    public static readonly BindableProperty IsLoadingProperty = BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(LoadingView), false);
    public bool IsLoading { get => (bool)GetValue(IsLoadingProperty); set => SetValue(IsLoadingProperty, value); }
    public LoadingView() => InitializeComponent();
}
