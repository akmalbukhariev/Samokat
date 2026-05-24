namespace Samokat.Views.Main.Components;

public partial class SearchBarView : ContentView
{
    public event Action? MenuClicked;
    public event Action? SearchClicked;
    public SearchBarView()
    {
        InitializeComponent();
    }

    private void LeftMenu_Clicked(object sender, EventArgs e)
    {
        MenuClicked?.Invoke();
    }

    private async void Search_Tapped(object sender, TappedEventArgs e)
    {
        Grid element = sender as Grid;
        await element.ScaleTo(0.9, 100, Easing.CubicOut);
        await element.ScaleTo(1.0, 100, Easing.CubicIn);

        SearchClicked?.Invoke();
    }
}