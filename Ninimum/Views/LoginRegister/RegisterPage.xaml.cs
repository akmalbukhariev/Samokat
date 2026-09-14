using Ninimum.ViewModels;

namespace Ninimum.Views.LoginRegister;

public partial class RegisterPage : BasePage, IQueryAttributable
{
    private readonly RegisterPageViewModel viewModel;

    public RegisterPage(RegisterPageViewModel viewModel)
    {
        InitializeComponent();

        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("PhoneNumber", out var phoneNumberValue))
            viewModel.PhoneNumber = phoneNumberValue?.ToString()?.Trim() ?? string.Empty;
    }
}
