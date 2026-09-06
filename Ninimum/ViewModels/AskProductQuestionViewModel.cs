using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models.Requests;
using Ninimum.Services;
using Utils;

namespace Ninimum.ViewModels;

[QueryProperty(nameof(ProductId), "productId")]
[QueryProperty(nameof(Title), "title")]
public partial class AskProductQuestionViewModel : ObservableObject
{
    private const int MaxQuestionLength = 1000;

    private readonly UserApiService apiService;
    private readonly AppControl appControl;

    [ObservableProperty] private long productId;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string questionText = string.Empty;
    [ObservableProperty] private bool isLoading;

    public string CharacterCountText => $"{QuestionText?.Length ?? 0} / {MaxQuestionLength}";
    public bool CanSubmit => !IsLoading && !string.IsNullOrWhiteSpace(QuestionText) && QuestionText.Trim().Length >= 3;

    public IAsyncRelayCommand SubmitCommand { get; }
    public IAsyncRelayCommand BackCommand { get; }

    public AskProductQuestionViewModel(UserApiService apiService, AppControl appControl)
    {
        this.apiService = apiService;
        this.appControl = appControl;

        SubmitCommand = new AsyncRelayCommand(SubmitAsync);
        BackCommand = new AsyncRelayCommand(() => AppNavigatorService.NavigateTo(".."));
    }

    partial void OnQuestionTextChanged(string value)
    {
        if (value?.Length > MaxQuestionLength)
        {
            QuestionText = value[..MaxQuestionLength];
            return;
        }

        OnPropertyChanged(nameof(CharacterCountText));
        OnPropertyChanged(nameof(CanSubmit));
    }

    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSubmit));
    }

    private async Task SubmitAsync()
    {
        if (IsLoading)
            return;

        if (!await appControl.EnsureAuthenticatedAsync())
            return;

        string question = QuestionText?.Trim() ?? string.Empty;
        if (question.Length < 3)
        {
            await AlertService.ShowAlertAsync("Ogohlantirish", "Iltimos, mahsulot bo'yicha savolingizni yozing.");
            return;
        }

        try
        {
            IsLoading = true;

            var response = await apiService.AddProductQuestion(new AddProductQuestionRequest
            {
                product_id = ProductId,
                question = question
            });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                await AlertService.ShowAlertAsync("Xatolik", response.resultMsg ?? "Savolni yuborib bo'lmadi.");
                return;
            }

            PageDataRefreshState.MarkDirty(PageDataRefreshState.ProductQuestions(ProductId));
            await AlertService.ShowAlertAsync("Muvaffaqiyatli", "Savolingiz yuborildi. Javob berilgach shu sahifada ko'rinadi.");
            await AppNavigatorService.NavigateTo("..");
        }
        catch (Exception ex)
        {
            await AlertService.ShowAlertAsync("Xatolik", $"Savolni yuborib bo'lmadi.\n{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
