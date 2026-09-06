using System.Collections.ObjectModel;
using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models.Requests;
using Models.Responses;
using Ninimum.Models;
using Ninimum.Services;
using Ninimum.Views.DetailProduct;
using Utils;

namespace Ninimum.ViewModels;

[QueryProperty(nameof(ProductId), "productId")]
[QueryProperty(nameof(Title), "title")]
public partial class ProductQuestionsViewModel : ObservableObject
{
    private readonly UserApiService apiService;
    private readonly AppControl appControl;
    private readonly SemaphoreSlim refreshLock = new(1, 1);

    [ObservableProperty] private long productId;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private ObservableCollection<ProductQuestionItem> questions = new();

    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand AskQuestionCommand { get; }
    public IAsyncRelayCommand BackCommand { get; }

    public string QuestionCountText => Questions.Count == 0 ? "Savollar yo'q" : $"{Questions.Count} ta savol";

    public ProductQuestionsViewModel(UserApiService apiService, AppControl appControl)
    {
        this.apiService = apiService;
        this.appControl = appControl;

        RefreshCommand = new AsyncRelayCommand(ManualRefreshAsync);
        AskQuestionCommand = new AsyncRelayCommand(AskQuestionAsync);
        BackCommand = new AsyncRelayCommand(() => AppNavigatorService.NavigateTo(".."));
    }

    public async Task LoadAsync(bool showLoading = true)
    {
        if (ProductId <= 0)
            return;

        await refreshLock.WaitAsync();

        try
        {
            if (showLoading)
                IsLoading = true;

            ProductQuestionListResponse response = await apiService.GetProductQuestionList(
                new ProductQuestionListRequest
                {
                    product_id = ProductId,
                    pageSize = 100,
                    offset = 0
                });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
                return;

            Questions.Clear();

            if (response.resultData != null)
            {
                foreach (var question in response.resultData)
                {
                    bool answered = string.Equals(question.status, "ANSWERED", StringComparison.OrdinalIgnoreCase)
                                    && !string.IsNullOrWhiteSpace(question.answer);

                    Questions.Add(new ProductQuestionItem
                    {
                        Id = question.id ?? 0,
                        CustomerName = string.IsNullOrWhiteSpace(question.customer_name)
                            ? "Foydalanuvchi"
                            : question.customer_name.Trim(),
                        Question = question.question?.Trim() ?? string.Empty,
                        Answer = question.answer?.Trim() ?? string.Empty,
                        CreatedDate = question.created_at?.ToString("dd.MM.yyyy") ?? string.Empty,
                        AnsweredDate = question.answered_at?.ToString("dd.MM.yyyy") ?? string.Empty,
                        IsAnswered = answered
                    });
                }
            }

            OnPropertyChanged(nameof(QuestionCountText));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductQuestions] {ex.Message}");
        }
        finally
        {
            if (showLoading)
                IsLoading = false;

            refreshLock.Release();
        }
    }

    private async Task ManualRefreshAsync()
    {
        AppVibrationService.Click();

        try
        {
            IsRefreshing = true;
            await LoadAsync(showLoading: false);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private async Task AskQuestionAsync()
    {
        if (!await appControl.EnsureAuthenticatedAsync())
            return;

        await AppNavigatorService.NavigateTo(
            $"{nameof(AskProductQuestionPage)}" +
            $"?productId={ProductId}" +
            $"&title={Uri.EscapeDataString(Title ?? string.Empty)}");
    }
}
