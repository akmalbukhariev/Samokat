
using System.ComponentModel;
using Ninimum.ViewModels;

namespace Ninimum.Views.Search;

public partial class SearchPage : BasePage
{
    private SearchPageViewModel? viewModel;
    private CancellationTokenSource? keyboardCts;
    private bool focusPending;
    public SearchPage(SearchPageViewModel vm) : this(vm, false) { }

    protected SearchPage(SearchPageViewModel vm, bool isTabRoot)
    {
        InitializeComponent();
        viewModel = vm;
        BindingContext = vm;

        Shell.SetTabBarIsVisible(this, isTabRoot);
        SearchHeader.ShowBack = !isTabRoot;

        Loaded += SearchPage_Loaded;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel?.RefreshSearchHistoryForDisplay();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        keyboardCts?.Cancel();
        keyboardCts?.Dispose();
        keyboardCts = new CancellationTokenSource();
        focusPending = true;
        FocusSearchWhenReady();
    }

    protected override void OnDisappearing()
    {
        focusPending = false;
        keyboardCts?.Cancel();
        base.OnDisappearing();
    }

    private void FocusSearchWhenReady()
    {
        if (!IsLoaded || !focusPending) return;
        Dispatcher.Dispatch(async () =>
        {
            if (!focusPending || keyboardCts == null || keyboardCts.IsCancellationRequested) return;
            focusPending = false;
            try
            {
                await SearchInput.FocusEntryAndShowKeyboardAsync(keyboardCts.Token);
            }
            catch (OperationCanceledException) { }
        });
    }

    private void SearchPage_Loaded(object? sender, EventArgs e)
    {
        FocusSearchWhenReady();
        if (BindingContext is SearchPageViewModel vm)
        {
            if (viewModel != null)
            {
                viewModel.OpenFilterRequested -= ViewModel_OpenFilterRequested;
                viewModel.CloseFilterRequested -= ViewModel_CloseFilterRequested;
            }

            viewModel = vm;
            viewModel.OpenFilterRequested += ViewModel_OpenFilterRequested;
            viewModel.CloseFilterRequested += ViewModel_CloseFilterRequested;
        }
    }

    private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.ShowLikedView) && viewModel.ShowLikedView)
        {
            await likeView.DisplayAsAnimation();
            viewModel.ShowLikedView = false;
        }

        if (e.PropertyName == nameof(viewModel.ShowCartView) && viewModel.ShowCartView)
        {
            await cartView.DisplayAsAnimation();
            viewModel.ShowCartView = false;
        }
    }

    private async void ViewModel_OpenFilterRequested()
    {
        await SearchFilterBottomSheetView.ShowAsync();
    }
    
    private async void ViewModel_CloseFilterRequested()
    {
        await SearchFilterBottomSheetView.HideAsync();
    }
}