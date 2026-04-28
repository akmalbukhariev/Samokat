namespace Samokat.Components;

public partial class QuestionPopupView : ContentView
{
    public event EventHandler? Confirmed;
    public event EventHandler? Closed;

    public static readonly BindableProperty IsSuccessProperty =
        BindableProperty.Create(nameof(IsSuccess), typeof(bool), typeof(QuestionPopupView), false);

    public static readonly BindableProperty QuestionTitleProperty =
        BindableProperty.Create(nameof(QuestionTitle), typeof(string), typeof(QuestionPopupView), string.Empty);

    public static readonly BindableProperty YesTextProperty =
        BindableProperty.Create(nameof(YesText), typeof(string), typeof(QuestionPopupView), "Yes");

    public static readonly BindableProperty NoTextProperty =
        BindableProperty.Create(nameof(NoText), typeof(string), typeof(QuestionPopupView), "No");

    public static readonly BindableProperty SuccessTitleProperty =
        BindableProperty.Create(nameof(SuccessTitle), typeof(string), typeof(QuestionPopupView), string.Empty);

    public static readonly BindableProperty SuccessMessageProperty =
        BindableProperty.Create(nameof(SuccessMessage), typeof(string), typeof(QuestionPopupView), string.Empty);

    public static readonly BindableProperty OkTextProperty =
        BindableProperty.Create(nameof(OkText), typeof(string), typeof(QuestionPopupView), "OK");

    public bool IsSuccess
    {
        get => (bool)GetValue(IsSuccessProperty);
        set => SetValue(IsSuccessProperty, value);
    }

    public string QuestionTitle
    {
        get => (string)GetValue(QuestionTitleProperty);
        set => SetValue(QuestionTitleProperty, value);
    }

    public string YesText
    {
        get => (string)GetValue(YesTextProperty);
        set => SetValue(YesTextProperty, value);
    }

    public string NoText
    {
        get => (string)GetValue(NoTextProperty);
        set => SetValue(NoTextProperty, value);
    }

    public string SuccessTitle
    {
        get => (string)GetValue(SuccessTitleProperty);
        set => SetValue(SuccessTitleProperty, value);
    }

    public string SuccessMessage
    {
        get => (string)GetValue(SuccessMessageProperty);
        set => SetValue(SuccessMessageProperty, value);
    }

    public string OkText
    {
        get => (string)GetValue(OkTextProperty);
        set => SetValue(OkTextProperty, value);
    }

    public QuestionPopupView()
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

    private void OnYesClicked(object sender, EventArgs e)
    {
        Confirmed?.Invoke(this, EventArgs.Empty);
    }

    private void OnNoClicked(object sender, EventArgs e)
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