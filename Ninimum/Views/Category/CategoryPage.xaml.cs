using Ninimum.Models;
using Ninimum.Models.Dto;
using Ninimum.Services;
using Ninimum.ViewModels;
using Ninimum.Views.DetailProduct;
namespace Ninimum.Views.Category;

public partial class CategoryPage : BasePage
{
    private readonly CategoryPageViewModel model;
    public CategoryPage(CategoryPageViewModel model)
    {
        InitializeComponent();
        this.model = model;
        BindingContext = model;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Refresh admin-managed categories on each visit, preserving the selection when possible.
        await model.RefreshAsync();
    }
    private async void Category_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is not VisualElement view || view.BindingContext is not CategoryItem item) return;
        if (model.SelectedCategory == item) return;
        if (model.Products.Count > 0)
            ProductList.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
        CategoryList.ScrollTo(item, position: ScrollToPosition.MakeVisible, animate: true);
        await model.SelectAsync(item);
    }
    private async void Product_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is not VisualElement view || view.BindingContext is not ProductDto product || !product.id.HasValue) return;
        await ClickGuard.RunAsync(this, async () =>
        {
            await view.ScaleToAsync(0.95, 100, Easing.CubicOut);
            await view.ScaleToAsync(1, 100, Easing.CubicIn);
            await AppNavigatorService.NavigateTo($"{nameof(DetailProductPage)}?productId={product.id.Value}");
        }, setInputTransparent: false);
    }
}
