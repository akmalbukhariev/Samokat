using System.Collections.ObjectModel;

namespace Samokat.Views.Formalization;

public partial class FormalizationPage : BasePage
{
    private DeliveryType _selectedDeliveryType = DeliveryType.Pickup;
    private PaymentType _selectedPaymentType = PaymentType.Online;

    private const int ProductPrice = 545000;
    private const int CourierPrice = 30000;

    public ObservableCollection<ProductItem> Products { get; } = new();
    public ObservableCollection<CardItem> Cards { get; } = new();

    public FormalizationPage()
    {
        InitializeComponent();
        BindingContext = this;

        LoadProducts();
        LoadCards();

        UpdateDeliveryUI();
        UpdatePaymentUI();
        UpdateSummaryUI();
    }

    private void LoadProducts()
    {
        Products.Clear();

        Products.Add(new ProductItem { ImageSource = "product_1.png" });
        Products.Add(new ProductItem { ImageSource = "product_2.png" });
    }

    private void LoadCards()
    {
        Cards.Clear();

        Cards.Add(new CardItem
        {
            CardNumber = "0000 * * 0000",
            IsSelected = true,
            IsAddCard = false
        });

        Cards.Add(new CardItem
        {
            CardNumber = "0000 * * 0000",
            IsSelected = false,
            IsAddCard = false
        });

        Cards.Add(new CardItem
        {
            CardNumber = string.Empty,
            IsSelected = false,
            IsAddCard = true
        });

        RefreshCardImages();
    }

    private void OnPickupTabTapped(object sender, TappedEventArgs e)
    {
        _selectedDeliveryType = DeliveryType.Pickup;
        UpdateDeliveryUI();
        UpdateSummaryUI();
    }

    private void OnCourierTabTapped(object sender, TappedEventArgs e)
    {
        _selectedDeliveryType = DeliveryType.Courier;
        UpdateDeliveryUI();
        UpdateSummaryUI();
    }

    private void OnOnlinePaymentTabTapped(object sender, TappedEventArgs e)
    {
        _selectedPaymentType = PaymentType.Online;
        UpdatePaymentUI();
    }

    private void OnCashPaymentTabTapped(object sender, TappedEventArgs e)
    {
        _selectedPaymentType = PaymentType.CashOnDelivery;
        UpdatePaymentUI();
    }

    private async void OnChangeAddressTapped(object sender, TappedEventArgs e)
    {
        await DisplayAlert("Manzil", "Manzilni o’zgartirish bosildi.", "OK");
    }

    private async void OnAddCardTapped(object sender, TappedEventArgs e)
    {
        Cards.Insert(Cards.Count - 1, new CardItem
        {
            CardNumber = "1111 * * 2222",
            IsSelected = false,
            IsAddCard = false
        });

        await DisplayAlert("Karta", "Yangi karta qo’shish bosildi.", "OK");
    }

    private void OnCardTapped(object sender, TappedEventArgs e)
    {
        if (_selectedPaymentType != PaymentType.Online)
            return;

        if (sender is not Element element)
            return;

        if (element.BindingContext is not CardItem tappedCard || tappedCard.IsAddCard)
            return;

        foreach (var card in Cards)
        {
            if (!card.IsAddCard)
                card.IsSelected = false;
        }

        tappedCard.IsSelected = true;
        RefreshCardImages();
    }

    private void RefreshCardImages()
    {
        foreach (var card in Cards)
        {
            if (!card.IsAddCard)
            {
                card.RadioImage = card.IsSelected
                    ? "card_check_radibutton.png"
                    : "card_uncheck_radibutton.png";
            }
        }
    }

    private void UpdateDeliveryUI()
    {
        bool isPickup = _selectedDeliveryType == DeliveryType.Pickup;
        bool isCourier = _selectedDeliveryType == DeliveryType.Courier;

        PickupTabBorder.BackgroundColor = isPickup ? Colors.White : Color.FromArgb("#F7F8FB");
        CourierTabBorder.BackgroundColor = isCourier ? Colors.White : Color.FromArgb("#F7F8FB");

        if (isPickup)
        {
            PickupTabSubtitleLabel.Text = "Bepul";
            PickupTabSubtitleLabel.TextColor = Color.FromArgb("#21C940");

            CourierTabSubtitleLabel.Text = "30 000 so’m";
            CourierTabSubtitleLabel.TextColor = Color.FromArgb("#96979B");

            AddressLabel.Text = "Qashqadaryo viloyati, Shahrisabz shahar, Mo’ljal: 25-maktab";
            DeliveryInfoLabel.Text = "Ertaga olib ketasiz. Biz sizga xabar beramiz";
        }
        else
        {
            PickupTabSubtitleLabel.Text = "Bepul";
            PickupTabSubtitleLabel.TextColor = Color.FromArgb("#21C940");

            CourierTabSubtitleLabel.Text = "30 000 so’m";
            CourierTabSubtitleLabel.TextColor = Color.FromArgb("#21C940");

            AddressLabel.Text = "Qashqadaryo viloyati, Shahrisabz shahar, Mo’ljal: 25-maktab";
            DeliveryInfoLabel.Text = "1 soatda yetkazib beramiz";
        }

        DeliveryInfoLabel.TextColor = Color.FromArgb("#7CB518");
    }

    private void UpdatePaymentUI()
    {
        bool isOnline = _selectedPaymentType == PaymentType.Online;
        bool isCash = _selectedPaymentType == PaymentType.CashOnDelivery;

        OnlinePaymentTabBorder.BackgroundColor = isOnline ? Colors.White : Color.FromArgb("#F7F8FB");
        CashPaymentTabBorder.BackgroundColor = isCash ? Colors.White : Color.FromArgb("#F7F8FB");

        CardsCollectionView.Opacity = isOnline ? 1.0 : 0.45;
        CardsCollectionView.IsEnabled = isOnline;
    }

    private void UpdateSummaryUI()
    {
        int deliveryPrice = _selectedDeliveryType == DeliveryType.Courier ? CourierPrice : 0;
        int total = ProductPrice + deliveryPrice;

        ProductsCountLabel.Text = $"Mahsulotlar soni: {Products.Count} ta";
        ProductsPriceLabel.Text = FormatSom(ProductPrice);
        DeliveryPriceLabel.Text = FormatSom(deliveryPrice);
        TotalPriceLabel.Text = FormatSom(total);
    }

    private static string FormatSom(int amount)
    {
        return string.Format("{0:N0} so’m", amount).Replace(",", " ");
    }

    private enum DeliveryType
    {
        Pickup,
        Courier
    }

    private enum PaymentType
    {
        Online,
        CashOnDelivery
    }
}

public class ProductItem
{
    public string ImageSource { get; set; } = string.Empty;
}

public class CardItem : BindableObject
{
    private bool _isSelected;
    private string _radioImage = string.Empty;

    public string CardNumber { get; set; } = string.Empty;
    public bool IsAddCard { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public string RadioImage
    {
        get => _radioImage;
        set
        {
            _radioImage = value;
            OnPropertyChanged();
        }
    }
}