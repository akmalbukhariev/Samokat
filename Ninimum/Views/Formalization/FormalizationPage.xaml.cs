using System.Collections.ObjectModel;
using Api.Services;
using Models.Requests;
using Models.Responses;
using Ninimum.Models.Dto;
using Ninimum.Services;
using Ninimum.Views.LoginRegister;
using Ninimum.Views.PaymentCard;
using Utils;

namespace Ninimum.Views.Formalization;

public partial class FormalizationPage : BasePage
{
    private const int ProductPrice = 545000;
    private const int DeliveryPrice = 0;
    private bool productsExpanded;

    private PaymentCardDto? selectedPaymentCard;
    public ObservableCollection<ProductItem> Products { get; } = new();
    private readonly UserApiService apiService;
    private readonly AppControl appControl;
    public FormalizationPage(AppControl appControl, UserApiService apiService)
    {
        InitializeComponent();

        this.appControl = appControl;
        this.apiService = apiService;
        BindingContext = this;

        LoadData();
        UpdateSummaryUI();
    }

    private void LoadData()
    {
        var data = FormalizationNavigationStore.Data;

        if (data == null)
        {
            DisplayAlert("Xatolik", "Buyurtma ma’lumotlari topilmadi.", "OK");
            return;
        }

        AddressLabel.Text = data.AddressText;

        Products.Clear();

        foreach (var product in data.Products)
        {
            Products.Add(new ProductItem
            {
                ProductId = product.ProductId,
                ImageSource = product.ImageSource,
                Name = product.Name,
                Quantity = product.Quantity,
                QuantityText = product.QuantityText,
                PriceText = product.PriceText
            });
        }

        UpdateSummaryUI();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        await LoadPaymentCardsAsync();
    }

    private async Task LoadPaymentCardsAsync()
    {
        try
        {
            imCard.IsVisible = false;
            loadingCard.IsVisible = true;
            loadingCard.IsRunning = true;

            PaymentCardLabel.Text = "Kartalar yuklanmoqda...";
            
            PaymentCardListResponse response =
                await apiService.GetPaymentCardList(
                    new PaymentCardListParam()
                    {
                        user_id = appControl.userDto.id ?? 0
                    });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                selectedPaymentCard = null;
                PaymentCardLabel.Text = "Karta ma’lumotlari olinmadi";
                return;
            }

            var cards = response.resultData ?? new List<PaymentCardDto>();

            PaymentCardPage.NavigationCards = cards;

            if (cards.Count == 0)
            {
                selectedPaymentCard = null;
                PaymentCardLabel.Text = "Karta tanlanmagan";
                return;
            }

            selectedPaymentCard =
                cards.FirstOrDefault(x => x.is_default)
                ?? cards.First();

            PaymentCardLabel.Text = $"**** **** **** {selectedPaymentCard.last_four_digits}";
        }
        catch
        {
            selectedPaymentCard = null;
            PaymentCardLabel.Text = "Karta ma’lumotlari olinmadi";
        }
        finally
        {
            imCard.IsVisible = true;
            loadingCard.IsVisible = false;
            loadingCard.IsRunning = false;
        }
    }
   
    private async void OnChangeAddressTapped(object sender, TappedEventArgs e)
    {
        await AnimateElementScaleDown(sender as Border);

        await AppNavigatorService.NavigateTo(nameof(AddressPage));
    }
    
    private void OnToggleProductsTapped(object sender, TappedEventArgs e)
    {
        productsExpanded = !productsExpanded;

        ProductDetailsCollectionView.IsVisible = productsExpanded;
        ProductDetailsCollectionView.HeightRequest = productsExpanded
            ? Products.Count * 85
            : 0;

        ProductsToggleLabel.Text = productsExpanded
            ? $"{Products.Count} ta mahsulot yopish"
            : $"{Products.Count} ta mahsulot ko‘rish";

        ProductsToggleIcon.Source = productsExpanded
            ? "ic_arrow_up.png"
            : "ic_arrow_down.png";
    }

    private async void OnPaymentMethodTapped(object sender, TappedEventArgs e)
    {
        await AnimateElementScaleDown(sender as Border);

        await AppNavigatorService.NavigateTo(nameof(PaymentCardPage));
    }

    private async void OnCreateOrderClicked(object sender, EventArgs e)
    {
        if (selectedPaymentCard == null)
        {
            await DisplayAlert(
                "To‘lov kartasi",
                "Iltimos, to‘lov kartasini qo‘shing yoki tanlang.",
                "OK");

            await AppNavigatorService.NavigateTo(nameof(PaymentCardPage));
            return;
        }

        await DisplayAlert(
            "OK",
            $"Tanlangan karta: **** **** **** {selectedPaymentCard.last_four_digits}",
            "OK");

        // Next:
        // CreateOrder API
        // Mock payment success
        // Clear cart
    }

    private void UpdateSummaryUI()
    {
        int total = ProductPrice + DeliveryPrice;

        ProductsCountLabel.Text = $"{Products.Count} ta";
        ProductsPriceLabel.Text = FormatSom(ProductPrice);
        DeliveryPriceLabel.Text = DeliveryPrice == 0 ? "Bepul" : FormatSom(DeliveryPrice);
        TotalPriceLabel.Text = FormatSom(total);
    }

    private static string FormatSom(int amount)
    {
        return string.Format("{0:N0} so’m", amount).Replace(",", " ");
    }
}

public class ProductItem
{
    public long ProductId { get; set; }
    public string ImageSource { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string QuantityText { get; set; } = string.Empty;
    public string PriceText { get; set; } = string.Empty;

    public string DetailText => $"{QuantityText} / {PriceText}";
}