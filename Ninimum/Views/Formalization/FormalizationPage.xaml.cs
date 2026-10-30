using Ninimum.Resources.Languages;
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
            DisplayAlert(AppResource.Error, AppResource.OrderInformationWasNotFound_80654d, AppResource.Ok);
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

        var data = FormalizationNavigationStore.Data;
        if (data != null)
            AddressLabel.Text = data.AddressText;

        try { await RefreshQuoteAsync(); } catch { /* Checkout retries and requires a successful fresh quote. */ }
        await LoadPaymentCardsAsync();
    }

    // Delivery method chosen on this page; taxi only when the server says it's within working hours.
    private string deliveryMethod = "COURIER";
    private bool taxiAvailable;

    private static string L(string uz, string ru, string en)
    {
        var lang = (AppResource.Culture ?? System.Globalization.CultureInfo.CurrentUICulture).TwoLetterISOLanguageName;
        return lang == "ru" ? ru : lang == "en" ? en : uz;
    }

    private void ShowDeliveryMethod()
    {
        DeliveryMethodTitle.Text = L("Yetkazish usuli", "Способ доставки", "Delivery method");
        CourierTitle.Text = L("Kuryer orqali", "Курьером", "By courier");
        CourierHint.Text = L("Bepul. Kuryer navbat bo‘yicha olib boradi.", "Бесплатно. Курьер доставит в порядке очереди.", "Free. A courier delivers in turn.");
        TaxiTitle.Text = L("Tezkor — taksi orqali", "Срочно — на такси", "Fast — by taxi");
        TaxiHint.Text = taxiAvailable
            ? L("Taksi haqini haydovchiga o‘zingiz to‘laysiz. Mahsulotlar uchun to‘lov o‘zgarmaydi.", "Стоимость такси вы оплачиваете водителю сами. Оплата за товары не меняется.", "You pay the taxi fare to the driver yourself. The price of the products doesn’t change.")
            : L("Faqat ish vaqtida mavjud.", "Доступно только в рабочее время.", "Available only during working hours.");
        if (!taxiAvailable && deliveryMethod == "TAXI") deliveryMethod = "COURIER";
        bool taxi = deliveryMethod == "TAXI";
        CourierCheck.Source = taxi ? "ic_uncheck.png" : "ic_check.png";
        TaxiCheck.Source = taxi ? "ic_check.png" : "ic_uncheck.png";
        CourierOption.Stroke = taxi ? Color.FromArgb("#DADBDD") : Color.FromArgb("#7CB518");
        TaxiOption.Stroke = taxi ? Color.FromArgb("#FFB300") : Color.FromArgb("#DADBDD");
        TaxiOption.Opacity = taxiAvailable ? 1 : 0.5;
        ShowDeliveryPrice();
    }

    // Courier: "Bepul" in green. Taxi: Ninimum charges nothing, but the customer pays the driver — say so in orange.
    private void ShowDeliveryPrice()
    {
        if (deliveryMethod == "TAXI")
        {
            DeliveryPriceLabel.Text = L("Taksi haqi haydovchiga", "Такси — оплата водителю", "Taxi fare to the driver");
            DeliveryPriceLabel.TextColor = Color.FromArgb("#B45309");
        }
        else
        {
            DeliveryPriceLabel.Text = DeliveryPrice == 0 ? AppResource.Free : FormatSom(DeliveryPrice);
            DeliveryPriceLabel.TextColor = Color.FromArgb("#7CB518");
        }
    }

    private void OnCourierTapped(object? sender, TappedEventArgs e) { deliveryMethod = "COURIER"; AppVibrationService.Click(); ShowDeliveryMethod(); }

    private void OnTaxiTapped(object? sender, TappedEventArgs e)
    {
        if (!taxiAvailable) return;
        deliveryMethod = "TAXI"; AppVibrationService.Click(); ShowDeliveryMethod();
    }

    private async Task<CheckoutQuoteData> RefreshQuoteAsync()
    {
        ShowDeliveryMethod();
        var data=FormalizationNavigationStore.Data??throw new InvalidOperationException();
        var response=await apiService.GetCheckoutQuote(new CreateOrderRequest {
            userId=data.UserId,tariffSubscriptionId=data.TariffSubscriptionId,
            products=data.Products.Select(p=>new CreateOrderProductRequest{productId=p.ProductId,quantity=p.Quantity}).ToList()
        });
        var quote=response?.resultData;
        if(response?.resultCode!="100"||quote==null||quote.total<=0||quote.products.Count!=data.Products.Count)throw new InvalidOperationException(AppResource.OrderCouldNotBeCreated);
        taxiAvailable=quote.taxi_available;ShowDeliveryMethod();
        for(int i=0;i<data.Products.Count;i++){
            if(quote.products[i].productId!=data.Products[i].ProductId||quote.products[i].quantity!=data.Products[i].Quantity)throw new InvalidOperationException();
            data.Products[i].Price=quote.products[i].price;
        }
        LoadData();
        return quote;
    }

    private async Task LoadPaymentCardsAsync()
    {
        try
        {
            imCard.IsVisible = false;
            loadingCard.IsVisible = true;
            loadingCard.IsRunning = true;

            PaymentCardLabel.Text = AppResource.PaymentCardsLoading;
            
            PaymentCardListResponse response =
                await apiService.GetPaymentCardList(
                    new PaymentCardListParam()
                    {
                        user_id = appControl.userDto.id ?? 0
                    });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                selectedPaymentCard = null;
                PaymentCardLabel.Text = AppResource.CouldNotLoadCardDetails;
                return;
            }

            var cards = response.resultData ?? new List<PaymentCardDto>();

            PaymentCardPage.NavigationCards = cards;

            if (cards.Count == 0)
            {
                selectedPaymentCard = null;
                PaymentCardLabel.Text = AppResource.NoCardSelected;
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
            PaymentCardLabel.Text = AppResource.CouldNotLoadCardDetails;
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

        var data = FormalizationNavigationStore.Data;

        AddressSelectionNavigationStore.Prepare(
            AddressSelectionMode.Checkout,
            data?.AddressText,
            data?.AddressLatitude,
            data?.AddressLongitude);

        await AppNavigatorService.NavigateTo(nameof(AddressPage));
    }
    
    private void OnToggleProductsTapped(object sender, TappedEventArgs e)
    {
        productsExpanded = !productsExpanded;
        UpdateProductsToggleUI();
    }

    private void UpdateProductsToggleUI()
    {
        ProductDetailsCollectionView.IsVisible = productsExpanded;
        ProductDetailsCollectionView.HeightRequest = productsExpanded
            ? Products.Count * 85
            : 0;

        ProductsToggleLabel.Text = productsExpanded
            ? string.Format(AppResource.HideProducts, Products.Count)
            : string.Format(AppResource.ViewProducts, Products.Count);

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

    private static string CheckoutStockMessage(string key) =>
        AppResource.ResourceManager.GetString(key, AppResource.Culture) ?? key;

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
                        AppResource.Error,
                        AppResource.OrderInformationWasNotFound_80654d,
                        AppResource.Ok);

                    return;
                }

                var displayedTotal=data.Products.Sum(p=>p.Price*p.Quantity);
                var checkoutQuote=await RefreshQuoteAsync();
                if(checkoutQuote.total!=displayedTotal) {
                    var lang=System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                    await Shell.Current.DisplayAlert(AppResource.Error,lang=="ru"?"Цена обновилась. Проверьте сумму и подтвердите заказ снова.":lang=="uz"?"Narx yangilandi. Summani tekshiring va buyurtmani qayta tasdiqlang.":"The price changed. Review the total and confirm again.",AppResource.Ok);
                    return;
                }

                // Always use fresh server stock before creating an order or opening Payme.
                // Group identical products so combined quantities cannot exceed stock.
                IsLoading = true;
                foreach (var group in data.Products.GroupBy(x => x.ProductId))
                {
                    var detail = await apiService.GetProductDetail(new ProductDetailRequest
                    {
                        user_id = appControl.CurrentUserId,
                        product_id = checked((int)group.Key)
                    });

                    if (detail.resultCode != ApiResult.SUCCESS.GetCodeToString())
                    {
                        await Shell.Current.DisplayAlert(
                            AppResource.Error,
                            CheckoutStockMessage("CheckoutStockCheckFailed"),
                            AppResource.Ok);
                        return;
                    }

                    var product = detail.resultData;
                    if (product == null || product.stock_quantity == 0)
                    {
                        PageDataRefreshState.MarkDirty(PageDataRefreshState.Cart);
                        PageDataRefreshState.MarkDirty(PageDataRefreshState.Main);
                        PageDataRefreshState.MarkDirty(PageDataRefreshState.DetailProduct(group.Key));
                        await Shell.Current.DisplayAlert(
                            AppResource.Error,
                            $"{group.First().Name}: {CheckoutStockMessage("CheckoutOutOfStock")}",
                            AppResource.Ok);
                        return;
                    }

                    if (!product.stock_quantity.HasValue || product.stock_quantity.Value < 0)
                    {
                        await Shell.Current.DisplayAlert(
                            AppResource.Error,
                            CheckoutStockMessage("CheckoutStockCheckFailed"),
                            AppResource.Ok);
                        return;
                    }

                    if (group.Any(x => x.Quantity <= 0) ||
                        group.Sum(x => (long)x.Quantity) > product.stock_quantity.Value)
                    {
                        PageDataRefreshState.MarkDirty(PageDataRefreshState.DetailProduct(group.Key));
                        await Shell.Current.DisplayAlert(
                            AppResource.Error,
                            $"{group.First().Name}: {string.Format(CheckoutStockMessage("CheckoutInsufficientStock"), product.stock_quantity.Value)}",
                            AppResource.Ok);
                        return;
                    }
                }

                int totalPrice = (int)data.Products.Sum(x => x.Price * x.Quantity);

                if (totalPrice <= 0)
                {
                    await Shell.Current.DisplayAlert(
                        AppResource.Error,
                        AppResource.TheOrderAmountIsInvalid,
                        AppResource.Ok);

                    return;
                }

                if (string.IsNullOrWhiteSpace(data.AddressText) ||
                    data.AddressLatitude is not double addressLatitude ||
                    data.AddressLongitude is not double addressLongitude ||
                    !double.IsFinite(addressLatitude) || !double.IsFinite(addressLongitude) ||
                    addressLatitude < 39.0000 || addressLatitude > 39.1500 ||
                    addressLongitude < 66.7500 || addressLongitude > 66.9500)
                {
                    await Shell.Current.DisplayAlert(AppResource.Address,
                        AppResource.PleaseSelectADeliveryAddress, AppResource.Ok);
                    return;
                }

                var createOrderRequest = new CreateOrderRequest
                {
                    userId = data.UserId,
                    deliveryAddress = data.AddressText.Trim(),
                    deliveryLatitude = addressLatitude,
                    deliveryLongitude = addressLongitude,
                    totalPrice = totalPrice,
                    quotedTotal = checkoutQuote.total,
                    tariffSubscriptionId = data.TariffSubscriptionId,
                    deliveryMethod = deliveryMethod,

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
                    $"tariffSubscriptionId={createOrderRequest.tariffSubscriptionId}, " +
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
                        AppResource.Error,
                        createOrderResponse.resultCode == "STOCK_UNAVAILABLE"
                            ? (createOrderResponse.availableQuantity > 0
                                ? string.Format(CheckoutStockMessage("CheckoutInsufficientStock"), createOrderResponse.availableQuantity)
                                : CheckoutStockMessage("CheckoutOutOfStock"))
                            : createOrderResponse.resultMsg ?? AppResource.OrderCouldNotBeCreated,
                        AppResource.Ok);

                    return;
                }

                if (createOrderResponse.resultData == null ||
                    createOrderResponse.resultData <= 0)
                {
                    await Shell.Current.DisplayAlert(
                        AppResource.Error,
                        AppResource.OrderIDWasNotReceived,
                        AppResource.Ok);

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
                    await CancelUnpaidCheckoutOrderAsync(orderId);

                    await Shell.Current.DisplayAlert(
                        AppResource.Error,
                        paymeResponse.resultMsg ?? AppResource.CouldNotGetThePaymePaymentURL,
                        AppResource.Ok);

                    return;
                }

                if (paymeResponse.resultData == null ||
                    string.IsNullOrWhiteSpace(paymeResponse.resultData.payment_url))
                {
                    await CancelUnpaidCheckoutOrderAsync(orderId);

                    await Shell.Current.DisplayAlert(
                        AppResource.Error,
                        AppResource.CouldNotGetThePaymePaymentURL,
                        AppResource.Ok);

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

    private async Task CancelUnpaidCheckoutOrderAsync(long orderId)
    {
        if (orderId <= 0)
            return;

        try
        {
            await apiService.CancelUnpaidOrder(new CancelOrderRequest
            {
                orderId = orderId,
                userId = appControl.CurrentUserId,
                reason = AppResource.CouldNotOpenThePaymePaymentPage
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ERROR] CancelUnpaidCheckoutOrderAsync => {ex}");
        }
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
        UpdateProductsToggleUI();

        int productsPrice = GetProductsPrice();
        int total = productsPrice + DeliveryPrice;

        ProductsCountLabel.Text = string.Format(AppResource.PiecesCount, Products.Sum(x => x.Quantity));
        ProductsPriceLabel.Text = FormatSom(productsPrice);
        ShowDeliveryPrice();
        TotalPriceLabel.Text = FormatSom(total);
    }

    private static string FormatSom(int amount)
    {
        return string.Format(AppResource.UZS_02854f, amount).Replace(",", " ");
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