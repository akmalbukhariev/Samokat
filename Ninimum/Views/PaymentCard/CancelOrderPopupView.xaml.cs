namespace Ninimum.Views.PaymentCard;

public partial class CancelOrderPopupView : ContentView
{
    public event EventHandler? Confirmed;
    public event EventHandler? Closed;

    public static readonly BindableProperty IsSuccessProperty =
        BindableProperty.Create(
            nameof(IsSuccess),
            typeof(bool),
            typeof(CancelOrderPopupView),
            false);

    public bool IsSuccess
    {
        get => (bool)GetValue(IsSuccessProperty);
        set => SetValue(IsSuccessProperty, value);
    }

    public CancelOrderPopupView()
    {
        InitializeComponent();
    }

    public void ShowConfirm()
    {
        IsSuccess = false;
        IsVisible = true;
    }

    public void ShowSuccess()
    {
        IsSuccess = true;
        IsVisible = true;
    }

    private void OnConfirmClicked(object sender, EventArgs e)
    {
        Confirmed?.Invoke(this, EventArgs.Empty);
    }

    private void OnDoNotCancelClicked(object sender, EventArgs e)
    {
        IsVisible = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }

    private void OnOkClicked(object sender, EventArgs e)
    {
        IsVisible = false;
        Closed?.Invoke(this, EventArgs.Empty);
    }
}