using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using System.Windows.Input;
using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models.Requests;
using Models.Responses;
using Ninimum.Models;
using Ninimum.Models.Dto;
using Ninimum.Models.Main;
using Ninimum.Services;
using Utils;

namespace Ninimum.ViewModels;

public partial class SearchPageViewModel : ObservableObject
{
    [ObservableProperty] private ICommand filterTapCommand;
    [ObservableProperty] private ICommand applyFilterCommand;
    [ObservableProperty] private ICommand selectSortCommand;
    [ObservableProperty] private ObservableCollection<MainProductCardItem> products;

    [ObservableProperty] private string minPrice = "";
    [ObservableProperty] private string maxPrice = "";

    [ObservableProperty] private string searchText = "";

    [ObservableProperty] private bool isSortCheapFirst = true;
    [ObservableProperty] private bool isSortExpensiveFirst;
    [ObservableProperty] private bool isSortNewestFirst;
    [ObservableProperty] private bool isSortOldestFirst;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private bool isSearching;

    public event Action? OpenFilterRequested;
    public event Action? CloseFilterRequested;
    private CancellationTokenSource? searchCts;

    private int offset = 0;
    private const int PageSize = 10;
    private bool hasMoreItems = true;
    private bool isRequestRunning = false;

    [ObservableProperty] private bool showRecentSearchList = true;
    [ObservableProperty] private bool showFilterSearchList = false;
    [ObservableProperty] private bool showProductResult = false;

    [ObservableProperty] private ObservableCollection<SearchHistoryItem> historyList = new();

    private readonly HashSet<long> loadedProductIds = new();
    private readonly UserApiService apiService;
    private readonly AppStoreService storeService;
    public SearchPageViewModel(UserApiService apiService, AppStoreService storeService)
    {
        this.apiService = apiService;
        this.storeService = storeService;

        FilterTapCommand = new Command(OnFilterTapped);
        ApplyFilterCommand = new Command(OnApplyFilterTapped);
        SelectSortCommand = new Command<string>(OnSelectSort);

        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        ClickHistoryCommand = new Command<SearchHistoryItem>(OnClickHistory);
        RemoveHistoryCommand = new Command<SearchHistoryItem>(OnRemoveHistory);

        Products = new ObservableCollection<MainProductCardItem>();

        LoadSearchHistory();
    }

    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand LoadMoreCommand{ get; }
    public ICommand ClickHistoryCommand { get; }
    public ICommand RemoveHistoryCommand { get; }

    private async Task LoadMoreAsync()
    {
        await SearchProductsAsync(SearchText, CancellationToken.None);
    }

    private async Task RefreshAsync()
    {
        searchCts?.Cancel();

        offset = 0;
        hasMoreItems = true;
        loadedProductIds.Clear();
        Products.Clear();

        IsRefreshing = true;

        try
        {
            await SearchProductsAsync(SearchText, CancellationToken.None, showMainLoading: false);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        searchCts?.Cancel();

        if (string.IsNullOrWhiteSpace(value))
        {
            ShowRecentSearchList = true;
            ShowFilterSearchList = false;
            ShowProductResult = false;

            Products.Clear();
            return;
        }

        ShowRecentSearchList = false;
        ShowFilterSearchList = true;
        ShowProductResult = false;

        searchCts = new CancellationTokenSource();
        _ = SearchWithDebounceAsync(value, searchCts.Token);
    }

    private async Task SearchWithDebounceAsync(string keyword, CancellationToken token)
    {
        try
        {
            await Task.Delay(500, token);

            if (token.IsCancellationRequested)
                return;

            offset = 0;
            hasMoreItems = true;
            loadedProductIds.Clear();
            Products.Clear();

            // keep filter list visible while API is searching
            ShowRecentSearchList = false;
            ShowFilterSearchList = true;
            ShowProductResult = false;
            IsLoading = false;

            try
            {
                IsSearching = true;
                await SearchProductsAsync(keyword, token, showMainLoading: false);
            }
            finally
            {
                IsSearching = false;
            }

            if (token.IsCancellationRequested)
                return;

            // after result received, show products
            ShowRecentSearchList = false;
            ShowFilterSearchList = false;
            ShowProductResult = true;

            AddSearchHistory(keyword);
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void AddSearchHistory(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return;

        keyword = keyword.Trim();

        // avoid saving one/two letters
        if (keyword.Length < 3)
            return;

        var exists = HistoryList.Any(x =>
            x.SearchedText.Equals(keyword, StringComparison.OrdinalIgnoreCase));

        if (exists)
            return;

        HistoryList.Insert(0, new SearchHistoryItem
        {
            SearchedText = keyword
        });

        while (HistoryList.Count > 10)
            HistoryList.RemoveAt(HistoryList.Count - 1);

        SaveSearchHistory();
    }

    private void LoadSearchHistory()
    {
        var list = storeService.Get<List<SearchHistoryItem>>(AppKeys.SearchHistoryKey, new List<SearchHistoryItem>());

        HistoryList.Clear();

        foreach (var item in list)
        {
            HistoryList.Add(item);
        }
    }

    private void SaveSearchHistory()
    {
        var list = HistoryList.ToList();
        storeService.Set(AppKeys.SearchHistoryKey, list);
    }

    private async void OnClickHistory(SearchHistoryItem item)
    {
        if (item == null)
        return;

        SearchText = item.SearchedText;

        searchCts?.Cancel();

        offset = 0;
        hasMoreItems = true;
        loadedProductIds.Clear();
        Products.Clear();

        ShowRecentSearchList = false;
        ShowFilterSearchList = false;
        ShowProductResult = true;

        await SearchProductsAsync(SearchText, CancellationToken.None);
    }

    private void OnRemoveHistory(SearchHistoryItem item)
    {
        if (item == null)
            return;

        HistoryList.Remove(item);
        SaveSearchHistory();
    }

    private async Task SearchProductsAsync(string keyword, CancellationToken token, bool showMainLoading = true)
    {
        if (isRequestRunning || !hasMoreItems)
            return;

        try
        {
            isRequestRunning = true;
            if (showMainLoading)
                IsLoading = true;

            var request = new SearchProductParam
            {
                keyword = keyword,
                pageSize = PageSize,
                offset = offset,
                minPrice = ParsePrice(MinPrice),
                maxPrice = ParsePrice(MaxPrice),
                sortType = GetSelectedSortType()
            };

            ProductResponse response = await apiService.SearchProductList(request);

            if (token.IsCancellationRequested)
                return;

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
                return;

            var items = response.resultData;

            if (items == null || items.Count == 0)
            {
                hasMoreItems = false;
                return;
            }

            foreach (var item in items)
            {
                if (item.id == null)
                    continue;

                if (loadedProductIds.Contains(item.id.Value))
                    continue;

                loadedProductIds.Add(item.id.Value);
                Products.Add(ToMainProductCardItem(item));
            }

            offset += items.Count;

            if (items.Count < PageSize)
                hasMoreItems = false;
        }
        finally
        {
            if (showMainLoading)
                IsLoading = false;
            isRequestRunning = false;
        }
    }

    private void OnFilterTapped()
    {
        OpenFilterRequested?.Invoke();
    }

    private async void OnApplyFilterTapped()
    {
        CloseFilterRequested?.Invoke();
        
        searchCts?.Cancel();

        AddSearchHistory(SearchText);

        offset = 0;
        hasMoreItems = true;
        loadedProductIds.Clear();
        Products.Clear();

        ShowRecentSearchList = false;
        ShowFilterSearchList = false;
        ShowProductResult = true;

        await SearchProductsAsync(SearchText, CancellationToken.None);
    }

    private void OnSelectSort(string sortType)
    {
        IsSortCheapFirst = sortType == "cheap";
        IsSortExpensiveFirst = sortType == "expensive";
        IsSortNewestFirst = sortType == "newest";
        IsSortOldestFirst = sortType == "oldest";
    }

    private double? ParsePrice(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = value.Replace(" ", "").Replace(",", "");

        if (double.TryParse(value, out double result) && result > 0)
            return result;

        return null;
    }

    private string GetSelectedSortType()
    {
        if (IsSortCheapFirst) return "cheap";
        if (IsSortExpensiveFirst) return "expensive";
        if (IsSortNewestFirst) return "newest";
        if (IsSortOldestFirst) return "oldest";

        return "newest";
    }

    private MainProductCardItem ToMainProductCardItem(ProductDto item)
    {
        var images = new ObservableCollection<MainProductImageItem>();

        if (item.images != null && item.images.Count > 0)
        {
            foreach (var img in item.images.OrderBy(x => x.sort_order ?? 0))
            {
                images.Add(new MainProductImageItem
                {
                    ImageSource = img.image_url
                });
            }
        }
        else
        {
            images.Add(new MainProductImageItem
            {
                ImageSource = "product_1.png"
            });
        }

        return new MainProductCardItem
        {
            Price = item.price?.ToString("N0").Replace(",", " ") ?? "0",
            Subscription_price = item.subscription_price?.ToString("N0").Replace(",", " ") ?? "0",
            Title = item.name ?? "",
            Rating = 4.8,
            ReviewCount = 301,
            ActionText = "+ Ertaga",
            Images = images
        };
    }
}