using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Ninimum.Models;

namespace Ninimum.ViewModels;

public partial class ProductReviewsViewModel : ObservableObject
{
    private int _quantity = 1;

    [ObservableProperty] private ICommand backCommand;
    [ObservableProperty] private ICommand filterTapCommand;
    [ObservableProperty] private ICommand applyFilterCommand;
    [ObservableProperty] private ICommand selectSortCommand;
    [ObservableProperty] private ICommand increaseCommand;
    [ObservableProperty] private ICommand decreaseCommand;

    [ObservableProperty] private ObservableCollection<string> buyerPhotos;
    [ObservableProperty] private ObservableCollection<ProductReviewItem> reviews;
    [ObservableProperty] private ObservableCollection<ProductReviewItem> allReviews;

    [ObservableProperty] private bool isSortNewestFirst = true;
    [ObservableProperty] private bool isSortRatingHighFirst;
    [ObservableProperty] private bool isSortRatingLowFirst;
    [ObservableProperty] private bool isPhotoOnly;

    public event Action? OpenFilterRequested;
    public event Action? BackRequested;

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }
    }

    public string BuyerPhotosCountText => $"{BuyerPhotos?.Count ?? 0} ta";

    // alias properties for XAML
    public bool IsNewestSelected
    {
        get => IsSortNewestFirst;
        set
        {
            if (IsSortNewestFirst != value)
            {
                IsSortNewestFirst = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsRatingHighSelected
    {
        get => IsSortRatingHighFirst;
        set
        {
            if (IsSortRatingHighFirst != value)
            {
                IsSortRatingHighFirst = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsRatingLowSelected
    {
        get => IsSortRatingLowFirst;
        set
        {
            if (IsSortRatingLowFirst != value)
            {
                IsSortRatingLowFirst = value;
                OnPropertyChanged();
            }
        }
    }

    public ProductReviewsViewModel()
    {
        BackCommand = new Command(OnBackTapped);
        FilterTapCommand = new Command(OnFilterTapped);
        ApplyFilterCommand = new Command(OnApplyFilterTapped);
        SelectSortCommand = new Command<string>(OnSelectSort);

        IncreaseCommand = new Command(() =>
        {
            Quantity++;
        });

        DecreaseCommand = new Command(() =>
        {
            if (Quantity > 1)
                Quantity--;
        });

        BuyerPhotos = new ObservableCollection<string>
        {
            "review1.png",
            "review2.png",
            "review3.png",
            "review4.png",
            "review5.png"
        };

        AllReviews = new ObservableCollection<ProductReviewItem>
        {
            new ProductReviewItem
            {
                CustomerName = "Eshmatov Toshmat",
                ReviewDate = "09.03.2026",
                ReviewDateValue = new DateTime(2026, 3, 9),
                Rating = 5,
                ReviewText = "Mahsulotga va Ninimum.uz tashkilotchilariga raxmat. Sifatli mahsulot tez va arzonga yetib keldi. Sizlarga ham Ninimum.uzni tavsiya qilaman. Bundan buyon faqat Ninimum.uzdan mahsulotlarni sotib olaman.",
                ReplyText = "Hurmatli Eshmatov Toshmat, xaridingiz uchun sizga o’z jamoamiz nomidan minnatdorchilik bildiramiz. Kelgusida bundanda yaxshiroq ishlashga harakat qilamiz.",
                Photos = new ObservableCollection<string>
                {
                    "review2.png",
                    "review3.png"
                }
            },
            new ProductReviewItem
            {
                CustomerName = "Mamatov Mamat",
                ReviewDate = "08.03.2026",
                ReviewDateValue = new DateTime(2026, 3, 8),
                Rating = 4,
                ReviewText = "Maxsumotni oldim. Hammasi yaxshi, menga yoqdi.",
                ReplyText = "Hurmatli Mamatov Mamat, xaridingiz uchun sizga o’z jamoamiz nomidan minnatdorchilik bildiramiz. Kelgusida bundanda yaxshiroq ishlashga harakat qilamiz.",
                Photos = new ObservableCollection<string>()
            }
        };

        Reviews = new ObservableCollection<ProductReviewItem>(AllReviews);
    }

    partial void OnBuyerPhotosChanged(ObservableCollection<string> value)
    {
        OnPropertyChanged(nameof(BuyerPhotosCountText));
    }

    partial void OnIsSortNewestFirstChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNewestSelected));
    }

    partial void OnIsSortRatingHighFirstChanged(bool value)
    {
        OnPropertyChanged(nameof(IsRatingHighSelected));
    }

    partial void OnIsSortRatingLowFirstChanged(bool value)
    {
        OnPropertyChanged(nameof(IsRatingLowSelected));
    }

    private void OnBackTapped()
    {
        BackRequested?.Invoke();
    }

    private void OnFilterTapped()
    {
        OpenFilterRequested?.Invoke();
    }

    private void OnApplyFilterTapped()
    {
        IEnumerable<ProductReviewItem> filtered = AllReviews;

        if (IsPhotoOnly)
            filtered = filtered.Where(x => x.HasPhotos);

        if (IsSortNewestFirst)
            filtered = filtered.OrderByDescending(x => x.ReviewDateValue);
        else if (IsSortRatingHighFirst)
            filtered = filtered.OrderByDescending(x => x.Rating);
        else if (IsSortRatingLowFirst)
            filtered = filtered.OrderBy(x => x.Rating);

        Reviews = new ObservableCollection<ProductReviewItem>(filtered);
    }

    private void OnSelectSort(string sortType)
    {
        IsSortNewestFirst = sortType == "newest";
        IsSortRatingHighFirst = sortType == "high";
        IsSortRatingLowFirst = sortType == "low";
    }
}