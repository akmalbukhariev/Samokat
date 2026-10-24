using Ninimum.Resources.Languages;
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
    [ObservableProperty] private bool isComplaint;
    public string ComplaintLabel => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch { "uz"=>"Bu shikoyat", "ru"=>"Это жалоба", _=>"This is a complaint" };
    public string ComplaintPrivacy => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch { "uz"=>"Shikoyat va javob faqat sizga va administratorga ko‘rinadi.", "ru"=>"Жалоба и ответ видны только вам и администратору.", _=>"Your complaint and its answer are visible only to you and the administrator." };

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

        if (string.IsNullOrWhiteSpace(question))
        {
            await AlertService.ShowAlertAsync(AppResource.Warning, AppResource.PleaseEnterAQuestion);
            return;
        }

        if (question.Length < 3)
        {
            await AlertService.ShowAlertAsync(AppResource.Warning, AppResource.TheQuestionMustContainAtLeast3Characters);
            return;
        }

        try
        {
            IsLoading = true;

            var response = await apiService.AddProductQuestion(new AddProductQuestionRequest
            {
                product_id = ProductId,
                kind = IsComplaint ? "COMPLAINT" : "QUESTION",
                question = question
            });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                await AlertService.ShowAlertAsync(AppResource.Error, response.resultMsg ?? AppResource.CouldNotSendTheQuestion);
                return;
            }

            PageDataRefreshState.MarkDirty(PageDataRefreshState.ProductQuestions(ProductId));
            await AlertService.ShowAlertAsync(AppResource.Success, AppResource.YourQuestionWasSentTheAnswerWillAppear);
            await AppNavigatorService.NavigateTo("..");
        }
        catch (Exception ex)
        {
            await AlertService.ShowAlertAsync(AppResource.Error, string.Format(AppResource.CouldNotSendTheQuestion_a01d00, global::Utils.ServerMessage.Exception(ex)));
        }
        finally
        {
            IsLoading = false;
        }
    }
}
