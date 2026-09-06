using Ninimum.ViewModels;

namespace Ninimum.Views.DetailProduct;

public partial class AskProductQuestionPage : BasePage
{
    public AskProductQuestionPage(AskProductQuestionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        Shell.SetTabBarIsVisible(this, false);
    }
}
