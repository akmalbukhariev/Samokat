using System.Collections.ObjectModel;
using System.Diagnostics;
using Api.Services;
using Models.Requests;
using Models.Responses;
using Ninimum.Models.Dto;
using Ninimum.Services;
using Ninimum.Views.LoginRegister;
using Ninimum.Views.Payment;
using Ninimum.Views.PaymentCard;
using Utils;

namespace Ninimum.Views.Formalization;

public partial class FormalizationPage : BasePage
{
    //private const int ProductPrice = 545000;
    private const int DeliveryPrice = 0;
    private bool productsExpanded;

    private PaymentCardDto? selectedPaymentCard;
    public ObservableCollection<ProductItem> Products { get; } = new();
    public bool IsLoading = false;
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
        await ClickGuard.RunAsync((VisualElement)sender, async () =>
        {
            await AnimateElementScaleDown(sender as Border);

            await AppNavigatorService.NavigateTo(nameof(PaymentCardPage));
        });
    }

    private async void OnCreateOrderClicked(object sender, EventArgs e)
    {
        await ClickGuard.RunAsync((VisualElement)sender, async () =>
        {
            try
            {
                AppVibrationService.Click();

                var data = FormalizationNavigationStore.Data;

                if (data == null || data.Products == null || !data.Products.Any())
                {
                    await Shell.Current.DisplayAlert(
                        "Xatolik",
                        "Buyurtma ma’lumotlari topilmadi.",
                        "OK");

                    return;
                }

                int totalPrice = (int)data.Products.Sum(x => x.Price * x.Quantity);

                if (totalPrice <= 0)
                {
                    await Shell.Current.DisplayAlert(
                        "Xatolik",
                        "Buyurtma summasi noto'g'ri.",
                        "OK");

                    return;
                }

                long addressId = 1;

                var createOrderRequest = new CreateOrderRequest
                {
                    userId = data.UserId,
                    addressId = addressId,
                    totalPrice = totalPrice,

                    products = data.Products
                        .Select(x => new CreateOrderProductRequest
                        {
                            productId = x.ProductId,
                            price = (int)x.Price,
                            quantity = x.Quantity
                        })
                        .ToList()
                };

                Debug.WriteLine(
                    $"CREATE ORDER => userId={createOrderRequest.userId}, " +
                    $"addressId={createOrderRequest.addressId}, " +
                    $"totalPrice={createOrderRequest.totalPrice}");

                foreach (var product in createOrderRequest.products)
                {
                    Debug.WriteLine(
                        $"ORDER PRODUCT => productId={product.productId}, " +
                        $"price={product.price}, " +
                        $"quantity={product.quantity}");
                }

                IsLoading = true;

                CreateOrderResponse createOrderResponse =
                    await apiService.CreateOrder(createOrderRequest);

                if (createOrderResponse.resultCode != ApiResult.SUCCESS.GetCodeToString())
                {
                    await Shell.Current.DisplayAlert(
                        "Xatolik",
                        createOrderResponse.resultMsg ?? "Buyurtma yaratilmadi.",
                        "OK");

                    return;
                }

                if (createOrderResponse.resultData == null ||
                    createOrderResponse.resultData <= 0)
                {
                    await Shell.Current.DisplayAlert(
                        "Xatolik",
                        "Buyurtma ID olinmadi.",
                        "OK");

                    return;
                }

                long orderId = createOrderResponse.resultData.Value;

                Debug.WriteLine(
                    $"ORDER CREATED SUCCESSFULLY => orderId={orderId}");

                var paymeRequest = new CreatePaymeCheckoutUrlRequest
                {
                    order_id = (int)orderId
                };

                CreatePaymeCheckoutUrlResponse paymeResponse =
                    await apiService.CreatePaymeCheckoutUrl(paymeRequest);

                if (paymeResponse.resultCode != ApiResult.SUCCESS.GetCodeToString())
                {
                    await Shell.Current.DisplayAlert(
                        "Xatolik",
                        paymeResponse.resultMsg ?? "Payme URL yaratilmadi.",
                        "OK");

                    return;
                }

                if (paymeResponse.resultData == null ||
                    string.IsNullOrWhiteSpace(paymeResponse.resultData.payment_url))
                {
                    await Shell.Current.DisplayAlert(
                        "Xatolik",
                        "Payme to'lov manzili olinmadi.",
                        "OK");

                    return;
                }

                string paymentUrl = paymeResponse.resultData.payment_url;

                Debug.WriteLine($"PAYME CHECKOUT URL => {paymentUrl}");

                var parameters = new ShellNavigationQueryParameters
                {
                    ["PaymentUrl"] = paymentUrl,
                    ["OrderId"] = orderId
                };

                await Shell.Current.GoToAsync(
                    nameof(PaymentPage),
                    parameters);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] CreateOrder: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        });
    }

    private int GetProductsPrice()
    {
        return Products.Sum(product => ParseSom(product.PriceText));
    }

    private static int ParseSom(string priceText)
    {
        if (string.IsNullOrWhiteSpace(priceText))
            return 0;

        string onlyNumbers = new string(priceText.Where(char.IsDigit).ToArray());

        return int.TryParse(onlyNumbers, out int price)
            ? price
            : 0;
    }

    private void UpdateSummaryUI()
    {
        int productsPrice = GetProductsPrice();
        int total = productsPrice + DeliveryPrice;

        ProductsCountLabel.Text = $"{Products.Count} ta";
        ProductsPriceLabel.Text = FormatSom(productsPrice);
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