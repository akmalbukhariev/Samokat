using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;
using Ninimum.Models.Startup;
using Ninimum.ViewModels;

namespace Ninimum.Views.DetailProduct;

public class TestImage
{
    public string Image { get; set; } = string.Empty;
    public TestImage(string image)
    {
        Image = image;
    }
}

public partial class DetailProductPage : BasePage
{
    private ObservableCollection<TestImage> productImages = new();
    public DetailProductPage()
    {
        InitializeComponent();

        BindingContext = this;

        productImages = new ObservableCollection<TestImage>
        {
            new TestImage("product_2.png"),
            new TestImage("product_2.png"),
            new TestImage("product_2.png"),
            new TestImage("product_2.png")
        };

        OnboardingCarousel.ItemsSource = productImages;

        UpdateBottomSection(0);
    }

    private async void OnSkipTapped(object sender, TappedEventArgs e)
    {
        await AnimateElementScaleDown(lbSkip);
    }

    private void OnCarouselPositionChanged(object sender, PositionChangedEventArgs e)
    {
        UpdateBottomSection(e.CurrentPosition);
    }

    private void OnStart()
    {

    }

    private void UpdateBottomSection(int position)
    {
         
    }

    private void UpdateCustomIndicator(int position)
    {
         
    }

}