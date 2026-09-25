using System.Globalization;
using Microsoft.Maui.Graphics;
using Utils;

namespace Ninimum.Views.Main.Components;

public partial class OrderProcessView : ContentView
{
    private static readonly Color ActiveColor = Color.FromArgb("#33CC66");
    private static readonly Color ActiveTextColor = Colors.White;
    private static readonly Color CompletedTextColor = Color.FromArgb("#D7F0E1");
    private static readonly Color InactiveNodeColor = Colors.White;
    private static readonly Color InactiveBorderColor = Color.FromArgb("#D5DBE3");
    private static readonly Color InactiveTextColor = Color.FromArgb("#C8D1DB");

    public OrderProcessView()
    {
        InitializeComponent();

        ApplyLocalizedText();
        ConvertStatusToStep();
        UpdateProgress();
        UpdateExpandState();
        UpdateOrderInfo();
    }

    public static readonly BindableProperty OrderNumberProperty =
        BindableProperty.Create(
            nameof(OrderNumber),
            typeof(string),
            typeof(OrderProcessView),
            string.Empty,
            propertyChanged: OnOrderInfoChanged);

    public string OrderNumber
    {
        get => (string)GetValue(OrderNumberProperty);
        set => SetValue(OrderNumberProperty, value);
    }

    public static readonly BindableProperty OrderStatusProperty =
        BindableProperty.Create(
            nameof(OrderStatus),
            typeof(string),
            typeof(OrderProcessView),
            "PENDING",
            propertyChanged: OnOrderStatusChanged);

    public string OrderStatus
    {
        get => (string)GetValue(OrderStatusProperty);
        set => SetValue(OrderStatusProperty, value);
    }

    public static readonly BindableProperty CurrentStepProperty =
        BindableProperty.Create(
            nameof(CurrentStep),
            typeof(OrderProcessStep),
            typeof(OrderProcessView),
            OrderProcessStep.PaymentCompleted,
            propertyChanged: OnStepChanged);

    public OrderProcessStep CurrentStep
    {
        get => (OrderProcessStep)GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(
            nameof(IsExpanded),
            typeof(bool),
            typeof(OrderProcessView),
            true,
            propertyChanged: OnExpandedChanged);

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly BindableProperty InternalToggleEnabledProperty =
        BindableProperty.Create(
            nameof(InternalToggleEnabled),
            typeof(bool),
            typeof(OrderProcessView),
            true);

    public bool InternalToggleEnabled
    {
        get => (bool)GetValue(InternalToggleEnabledProperty);
        set => SetValue(InternalToggleEnabledProperty, value);
    }

    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(
            nameof(IsLoading),
            typeof(bool),
            typeof(OrderProcessView),
            false);

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    private static void OnOrderStatusChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is not OrderProcessView view)
            return;

        view.ConvertStatusToStep();
        view.UpdateOrderInfo();
    }

    private static void OnOrderInfoChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is OrderProcessView view)
            view.UpdateOrderInfo();
    }

    private static void OnStepChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is OrderProcessView view)
            view.UpdateProgress();
    }

    private static void OnExpandedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is OrderProcessView view)
            view.UpdateExpandState();
    }

    private void ConvertStatusToStep()
    {
        switch (OrderStatus?.ToUpperInvariant())
        {
            case "PENDING":
                CurrentStep = OrderProcessStep.PaymentCompleted;
                break;
            case "CONFIRMED":
                CurrentStep = OrderProcessStep.ProductPreparing;
                break;
            case "PREPARING":
                CurrentStep = OrderProcessStep.DeliveryPreparing;
                break;
            case "ON_THE_WAY":
                CurrentStep = OrderProcessStep.OutForDelivery;
                break;
            case "DELIVERED":
                CurrentStep = OrderProcessStep.Delivered;
                break;
            default:
                CurrentStep = OrderProcessStep.PaymentCompleted;
                break;
        }
    }

    private void ApplyLocalizedText()
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

        switch (language)
        {
            case "ru":
                Step1Label.Text = "Оплачено";
                Step2Label.Text = "Товар\nготовится";
                Step3Label.Text = "Готов к\nдоставке";
                Step4Label.Text = "Доставляется";
                Step5Label.Text = "Доставлено";
                LoadingLabel.Text = "Загружается статус заказа...";
                break;

            case "en":
                Step1Label.Text = "Paid";
                Step2Label.Text = "Product\npreparing";
                Step3Label.Text = "Ready for\ndelivery";
                Step4Label.Text = "Out for\ndelivery";
                Step5Label.Text = "Delivered";
                LoadingLabel.Text = "Loading order status...";
                break;

            default:
                Step1Label.Text = "To'lov\nqilindi";
                Step2Label.Text = "Mahsulot\ntayyorlanmoqda";
                Step3Label.Text = "Yetkazishga\ntayyor";
                Step4Label.Text = "Yetkazil\nmoqda";
                Step5Label.Text = "Yetkazildi";
                LoadingLabel.Text = "Buyurtma holati yuklanmoqda...";
                break;
        }
    }

    private void UpdateOrderInfo()
    {
        var orderPrefix = string.IsNullOrWhiteSpace(OrderNumber) ? "" : $"#{OrderNumber} ";
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

        if (language == "ru")
        {
            switch (OrderStatus?.ToUpperInvariant())
            {
                case "PENDING":
                    TitleLabel.Text = "Оплата принята";
                    SubtitleLabel.Text = $"{orderPrefix}оплата за заказ успешно принята.";
                    break;
                case "CONFIRMED":
                    TitleLabel.Text = "Товар готовится";
                    SubtitleLabel.Text = $"{orderPrefix}заказ комплектуется.";
                    break;
                case "PREPARING":
                    TitleLabel.Text = "Готов к доставке";
                    SubtitleLabel.Text = $"{orderPrefix}заказ готов к передаче курьеру.";
                    break;
                case "ON_THE_WAY":
                    TitleLabel.Text = "Доставим сегодня";
                    SubtitleLabel.Text = $"{orderPrefix}заказ уже в пути.";
                    break;
                case "DELIVERED":
                    TitleLabel.Text = "Заказ доставлен";
                    SubtitleLabel.Text = $"{orderPrefix}заказ успешно доставлен.";
                    break;
                case "CANCELLED":
                    TitleLabel.Text = "Заказ отменён";
                    SubtitleLabel.Text = $"{orderPrefix}заказ отменён.";
                    break;
                default:
                    TitleLabel.Text = "Статус заказа";
                    SubtitleLabel.Text = string.IsNullOrWhiteSpace(OrderNumber) ? string.Empty : $"Номер заказа: {orderPrefix.Trim()}";
                    break;
            }

            return;
        }

        if (language == "en")
        {
            switch (OrderStatus?.ToUpperInvariant())
            {
                case "PENDING":
                    TitleLabel.Text = "Payment received";
                    SubtitleLabel.Text = $"{orderPrefix}payment for the order was received successfully.";
                    break;
                case "CONFIRMED":
                    TitleLabel.Text = "Product is being prepared";
                    SubtitleLabel.Text = $"{orderPrefix}order is being prepared.";
                    break;
                case "PREPARING":
                    TitleLabel.Text = "Ready for delivery";
                    SubtitleLabel.Text = $"{orderPrefix}order is ready to be handed to the courier.";
                    break;
                case "ON_THE_WAY":
                    TitleLabel.Text = "Arriving today";
                    SubtitleLabel.Text = $"{orderPrefix}order is now on the way.";
                    break;
                case "DELIVERED":
                    TitleLabel.Text = "Order delivered";
                    SubtitleLabel.Text = $"{orderPrefix}order was delivered successfully.";
                    break;
                case "CANCELLED":
                    TitleLabel.Text = "Order cancelled";
                    SubtitleLabel.Text = $"{orderPrefix}order was cancelled.";
                    break;
                default:
                    TitleLabel.Text = "Order status";
                    SubtitleLabel.Text = string.IsNullOrWhiteSpace(OrderNumber) ? string.Empty : $"Order number: {orderPrefix.Trim()}";
                    break;
            }

            return;
        }

        switch (OrderStatus?.ToUpperInvariant())
        {
            case "PENDING":
                TitleLabel.Text = "To'lov qabul qilindi";
                SubtitleLabel.Text = $"{orderPrefix}buyurtma uchun to'lov muvaffaqiyatli qabul qilindi.";
                break;
            case "CONFIRMED":
                TitleLabel.Text = "Mahsulot tayyorlanmoqda";
                SubtitleLabel.Text = $"{orderPrefix}buyurtma yig'ilish jarayonida.";
                break;
            case "PREPARING":
                TitleLabel.Text = "Yetkazishga tayyor";
                SubtitleLabel.Text = $"{orderPrefix}buyurtma kuryerga topshirishga tayyor.";
                break;
            case "ON_THE_WAY":
                TitleLabel.Text = "Bugun yetib boradi";
                SubtitleLabel.Text = $"{orderPrefix}buyurtma hozir yo'lda.";
                break;
            case "DELIVERED":
                TitleLabel.Text = "Buyurtma yetkazildi";
                SubtitleLabel.Text = $"{orderPrefix}buyurtma muvaffaqiyatli yetkazildi.";
                break;
            case "CANCELLED":
                TitleLabel.Text = "Buyurtma bekor qilindi";
                SubtitleLabel.Text = $"{orderPrefix}buyurtma bekor qilindi.";
                break;
            default:
                TitleLabel.Text = "Buyurtma holati";
                SubtitleLabel.Text = string.IsNullOrWhiteSpace(OrderNumber) ? string.Empty : $"Buyurtma raqami: {orderPrefix.Trim()}";
                break;
        }
    }

    private void UpdateProgress()
    {
        var stepIndex = (int)CurrentStep;

        UpdateNode(Step1Node, Step1Icon, Step1Label, 1, stepIndex, "order_step_payment_active.png", "order_step_payment_inactive.png");
        UpdateNode(Step2Node, Step2Icon, Step2Label, 2, stepIndex, "order_step_product_active.png", "order_step_product_inactive.png");
        UpdateNode(Step3Node, Step3Icon, Step3Label, 3, stepIndex, "order_step_ready_active.png", "order_step_ready_inactive.png");
        UpdateNode(Step4Node, Step4Icon, Step4Label, 4, stepIndex, "order_step_delivery_active.png", "order_step_delivery_inactive.png");
        UpdateNode(Step5Node, Step5Icon, Step5Label, 5, stepIndex, "order_step_delivered_active.png", "order_step_delivered_inactive.png");

        UpdateConnector(Connector1Solid, Connector1Dots, stepIndex >= 2);
        UpdateConnector(Connector2Solid, Connector2Dots, stepIndex >= 3);
        UpdateConnector(Connector3Solid, Connector3Dots, stepIndex >= 4);
        UpdateConnector(Connector4Solid, Connector4Dots, stepIndex >= 5);
    }

    private static void UpdateConnector(BoxView solidLine, Grid dots, bool isCompleted)
    {
        solidLine.IsVisible = isCompleted;
        dots.IsVisible = !isCompleted;
        dots.Opacity = 1;
    }

    private static void UpdateNode(Border node, Image icon, Label label, int nodeIndex, int currentStepIndex, string activeSource, string inactiveSource)
    {
        var isCompleted = nodeIndex < currentStepIndex;
        var isCurrent = nodeIndex == currentStepIndex;
        var isActive = isCompleted || isCurrent;

        node.BackgroundColor = isActive ? ActiveColor : InactiveNodeColor;
        node.Stroke = isActive ? ActiveColor : InactiveBorderColor;
        node.StrokeThickness = isActive ? 0 : 1;

        icon.Source = isActive ? activeSource : inactiveSource;

        label.TextColor = isCurrent ? ActiveTextColor : isCompleted ? CompletedTextColor : InactiveTextColor;
        label.FontAttributes = isCurrent ? FontAttributes.Bold : FontAttributes.None;
        label.Opacity = isCompleted || isCurrent ? 1 : 0.9;
    }

    private void UpdateExpandState()
    {
        ProgressContainer.IsVisible = IsExpanded;
        ToggleImage.Source = IsExpanded ? "ic_arrow_up.png" : "ic_arrow_down.png";
    }

    private void OnToggleTapped(object sender, TappedEventArgs e)
    {
        if (!InternalToggleEnabled)
            return;

        IsExpanded = !IsExpanded;
    }
}
