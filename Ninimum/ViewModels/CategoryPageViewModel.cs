using System.Collections.ObjectModel;
using Api.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Models.Requests;
using Ninimum.Models;
using Ninimum.Models.Dto;
using Ninimum.Resources.Languages;
using Ninimum.Services;
using Utils;

namespace Ninimum.ViewModels;

public partial class CategoryPageViewModel : ObservableObject
{
    private readonly UserApiService api;
    private readonly AppControl app;
    private int generation;
    private int offset;
    private bool hasMore;
    private bool refreshing;
    private const int PageSize = 20;
    public ObservableCollection<CategoryItem> Categories { get; } = new();
    public ObservableCollection<CategoryItem> Children { get; } = new();
    public ObservableCollection<ProductDto> Products { get; } = new();
    [ObservableProperty] private CategoryItem? selectedCategory;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private bool hasError;
    [ObservableProperty] private bool isEmpty;
    [ObservableProperty] private string emptyText = "";
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand LoadMoreCommand { get; }
    public IAsyncRelayCommand RetryCommand { get; }
    public IAsyncRelayCommand<CategoryItem> SelectCategoryCommand { get; }

    public CategoryPageViewModel(UserApiService api, AppControl app)
    {
        this.api = api;
        this.app = app;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        LoadMoreCommand = new AsyncRelayCommand(LoadMoreAsync);
        RetryCommand = new AsyncRelayCommand(RefreshAsync);
        SelectCategoryCommand = new AsyncRelayCommand<CategoryItem>(SelectAsync,
            AsyncRelayCommandOptions.AllowConcurrentExecutions);
    }

    public async Task RefreshAsync()
    {
        if (refreshing) return;
        refreshing = true;
        int current = ++generation;
        IsLoading = true;
        HasError = false;
        IsEmpty = false;
        int? selectedId = SelectedCategory?.categoryId;
        try
        {
            var response = await api.GetCategories();
            if (current != generation) return;
            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                HasError = true;
                return;
            }
            Categories.Clear();
            foreach (var item in response.resultData ?? new List<CategoryItem>())
                if (item.categoryId > 0) Categories.Add(item);
            var selection = Categories.FirstOrDefault(x => x.categoryId == selectedId) ?? Categories.FirstOrDefault();
            await SelectAsync(selection);
        }
        catch
        {
            if (current == generation) HasError = true;
        }
        finally
        {
            refreshing = false;
            IsRefreshing = false;
            if (current == generation) IsLoading = false;
        }
    }

    public async Task SelectAsync(CategoryItem? category)
    {
        // A generation token prevents a slow previous category request replacing the latest selection.
        int current = ++generation;
        SelectedCategory = category;
        foreach (var item in Categories) item.IsSelected = item == category;
        Children.Clear();
        if (category != null)
            foreach (var child in Categories.Where(x => x.parentId == category.categoryId && x != category))
                Children.Add(child);
        Products.Clear();
        offset = 0;
        hasMore = category != null;
        HasError = false;
        IsEmpty = false;
        if (category == null)
        {
            IsLoading = false;
            EmptyText = CatalogText.EmptyCategories;
            IsEmpty = true;
            return;
        }
        IsLoading = true;
        await LoadProductsAsync(category.categoryId, current);
    }

    private async Task LoadMoreAsync()
    {
        if (IsLoading || refreshing || !hasMore || SelectedCategory == null) return;
        IsLoading = true;
        await LoadProductsAsync(SelectedCategory.categoryId, generation);
    }

    private async Task LoadProductsAsync(int categoryId, int current)
    {
        try
        {
            var response = await api.GetProductList(new ProductListRequest
            {
                category_id = categoryId, user_id = app.CurrentUserId, include_subcategories = true,
                pageSize = PageSize, offset = offset
            });
            if (current != generation) return;
            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                HasError = true;
                return;
            }
            var items = response.resultData ?? new List<ProductDto>();
            foreach (var item in items)
                if (item.id.HasValue && !Products.Any(x => x.id == item.id)) Products.Add(item);
            offset += items.Count;
            hasMore = items.Count == PageSize;
            EmptyText = CatalogText.EmptyProducts;
            IsEmpty = Products.Count == 0;
        }
        catch
        {
            if (current == generation) HasError = true;
        }
        finally
        {
            if (current == generation) IsLoading = false;
        }
    }
}
