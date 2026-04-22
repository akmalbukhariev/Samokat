
using Samokat.ViewModels;

namespace Samokat.Views.FavoriteProduct;

public partial class FavoritePage : BasePage
{
    private FavoritePageViewModel viewModel;
    public FavoritePage(FavoritePageViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;
    }
}