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

    private void UpdateOrderInfo()
    {
        var orderPrefix = string.IsNullOrWhiteSpace(OrderNumber) ? "" : $"#{OrderNumber} ";

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
                TitleLabel.Text = "Yetkazishga tayyorlanmoqda";
                SubtitleLabel.Text = $"{orderPrefix}buyurtma kuryerga topshirish uchun tayyorlanmoqda.";
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
