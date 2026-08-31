using Utils;

namespace Ninimum.Views.Main.Components;

public partial class OrderProcessView : ContentView
{
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
            OrderProcessStep.OrderReceived,
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


    private static void OnOrderStatusChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is not OrderProcessView view)
            return;

        view.ConvertStatusToStep();
        view.UpdateOrderInfo();
    }


    private static void OnOrderInfoChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is OrderProcessView view)
            view.UpdateOrderInfo();
    }


    private static void OnStepChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is OrderProcessView view)
            view.UpdateProgress();
    }


    private static void OnExpandedChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is OrderProcessView view)
            view.UpdateExpandState();
    }


    private void ConvertStatusToStep()
    {
        switch (OrderStatus?.ToUpperInvariant())
        {
            case "PENDING":
            case "CONFIRMED":
                CurrentStep = OrderProcessStep.OrderReceived;
                break;

            case "PREPARING":
                CurrentStep = OrderProcessStep.Preparing;
                break;

            case "ON_THE_WAY":
            case "DELIVERED":
                CurrentStep = OrderProcessStep.OutForDelivery;
                break;

            default:
                CurrentStep = OrderProcessStep.OrderReceived;
                break;
        }
    }


    private void UpdateOrderInfo()
    {
        string number =
            string.IsNullOrWhiteSpace(OrderNumber)
                ? string.Empty
                : OrderNumber;

        switch (OrderStatus?.ToUpperInvariant())
        {
            case "PENDING":
                TitleLabel.Text =
                    $"{number} sonli buyurtma qabul qilindi";

                SubtitleLabel.Text =
                    "Buyurtmangiz holati tekshirilmoqda.";
                break;


            case "CONFIRMED":
                TitleLabel.Text =
                    $"{number} sonli buyurtma qabul qilindi";

                SubtitleLabel.Text =
                    "To‘lov muvaffaqiyatli amalga oshirildi. Buyurtmangiz tez orada tayyorlanadi.";
                break;


            case "PREPARING":
                TitleLabel.Text =
                    $"{number} sonli buyurtma tayyorlanmoqda";

                SubtitleLabel.Text =
                    "Mahsulotlaringiz yetkazib berish uchun tayyorlanmoqda.";
                break;


            case "ON_THE_WAY":
                TitleLabel.Text =
                    $"{number} sonli buyurtma yo‘lda";

                SubtitleLabel.Text =
                    "Buyurtmangiz sizga yetkazib berilmoqda.";
                break;


            case "DELIVERED":
                TitleLabel.Text =
                    $"{number} sonli buyurtma yetkazib berildi";

                SubtitleLabel.Text =
                    "Buyurtmangiz muvaffaqiyatli yetkazib berildi.";
                break;


            default:
                TitleLabel.Text =
                    $"{number} sonli buyurtma";

                SubtitleLabel.Text =
                    string.Empty;
                break;
        }
    }


    private void UpdateProgress()
    {
        Step1Circle.Source = "ic_empty_circle.png";
        Step2Circle.Source = "ic_empty_circle.png";
        Step3Circle.Source = "ic_empty_circle.png";

        Line1.Source = "ic_dot_line.png";
        Line2.Source = "ic_dot_line.png";


        switch (CurrentStep)
        {
            case OrderProcessStep.OrderReceived:

                Step1Circle.Source =
                    "ic_fill_circle.png";

                break;


            case OrderProcessStep.Preparing:

                Step1Circle.Source =
                    "ic_fill_circle.png";

                Step2Circle.Source =
                    "ic_fill_circle.png";

                Line1.Source =
                    "ic_solid_line.png";

                break;


            case OrderProcessStep.OutForDelivery:

                Step1Circle.Source =
                    "ic_fill_circle.png";

                Step2Circle.Source =
                    "ic_fill_circle.png";

                Step3Circle.Source =
                    "ic_fill_circle.png";

                Line1.Source =
                    "ic_solid_line.png";

                Line2.Source =
                    "ic_solid_line.png";

                break;
        }
    }


    private void UpdateExpandState()
    {
        ProgressContainer.IsVisible = IsExpanded;

        ToggleImage.Source =
            IsExpanded
                ? "ic_arrow_up.png"
                : "ic_arrow_down.png";
    }


    private void OnToggleTapped(
        object sender,
        TappedEventArgs e)
    {
        if (!InternalToggleEnabled)
            return;

        IsExpanded = !IsExpanded;
    }
}