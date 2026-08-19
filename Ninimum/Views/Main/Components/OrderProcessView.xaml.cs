using Utils;

namespace Ninimum.Views.Main.Components;

public partial class OrderProcessView : ContentView
{
    public OrderProcessView()
    {
        InitializeComponent();

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
        {
            view.UpdateOrderInfo();
        }
    }


    private static void OnStepChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is OrderProcessView view)
        {
            view.UpdateProgress();
        }
    }


    private static void OnExpandedChanged(
        BindableObject bindable,
        object oldValue,
        object newValue)
    {
        if (bindable is OrderProcessView view)
        {
            view.UpdateExpandState();
        }
    }


    private void ConvertStatusToStep()
    {
        switch (OrderStatus?.ToUpperInvariant())
        {
            case "PENDING":
            case "CONFIRMED":
                CurrentStep =
                    OrderProcessStep.OrderReceived;
                break;

            case "PREPARING":
                CurrentStep =
                    OrderProcessStep.Preparing;
                break;

            case "ON_THE_WAY":
                CurrentStep =
                    OrderProcessStep.OutForDelivery;
                break;

            case "DELIVERED":
                CurrentStep =
                    OrderProcessStep.Delivered;
                break;

            case "CANCELLED":
                CurrentStep =
                    OrderProcessStep.OrderReceived;
                break;
        }
    }


    private void UpdateOrderInfo()
    {
        string number =
            string.IsNullOrWhiteSpace(OrderNumber)
                ? ""
                : OrderNumber;

        switch (OrderStatus?.ToUpperInvariant())
        {
            case "PENDING":
                TitleLabel.Text =
                    $"{number} sonli buyurtma qabul qilindi";
                break;

            case "CONFIRMED":
                TitleLabel.Text =
                    $"{number} sonli buyurtma tasdiqlandi";
                break;

            case "PREPARING":
                TitleLabel.Text =
                    $"{number} sonli buyurtma yig’ish jarayonida";
                break;

            case "ON_THE_WAY":
                TitleLabel.Text =
                    $"{number} sonli buyurtma yetkazib berilmoqda";
                break;

            case "DELIVERED":
                TitleLabel.Text =
                    $"{number} sonli buyurtma yetkazib berildi";
                break;

            case "CANCELLED":
                TitleLabel.Text =
                    $"{number} sonli buyurtma bekor qilindi";
                break;

            default:
                TitleLabel.Text =
                    $"{number} sonli buyurtma";
                break;
        }
    }


    private void UpdateProgress()
    {
        Step1Circle.Source = "ic_empty_circle.png";
        Step2Circle.Source = "ic_empty_circle.png";
        Step3Circle.Source = "ic_empty_circle.png";
        Step4Circle.Source = "ic_empty_circle.png";

        Line1.Source = "ic_dot_line.png";
        Line2.Source = "ic_dot_line.png";
        Line3.Source = "ic_dot_line.png";

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


            case OrderProcessStep.Delivered:

                Step1Circle.Source =
                    "ic_fill_circle.png";

                Step2Circle.Source =
                    "ic_fill_circle.png";

                Step3Circle.Source =
                    "ic_fill_circle.png";

                Step4Circle.Source =
                    "ic_fill_circle.png";

                Line1.Source =
                    "ic_solid_line.png";

                Line2.Source =
                    "ic_solid_line.png";

                Line3.Source =
                    "ic_solid_line.png";

                break;
        }
    }


    private void UpdateExpandState()
    {
        ProgressContainer.IsVisible =
            IsExpanded;

        ToggleImage.Source =
            IsExpanded
                ? "ic_arrow_up.png"
                : "ic_arrow_down.png";
    }


    private void OnToggleTapped(
        object sender,
        TappedEventArgs e)
    {
        IsExpanded = !IsExpanded;
    }
}