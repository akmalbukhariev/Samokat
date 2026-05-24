using Utils;

namespace Samokat.Views.Main.Components;

public partial class OrderProcessView : ContentView
{
    public OrderProcessView()
    {
        InitializeComponent();
        UpdateProgress();
        UpdateExpandState();
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
                Step1Circle.Source = "ic_fill_circle.png";
                break;

            case OrderProcessStep.Preparing:
                Step1Circle.Source = "ic_fill_circle.png";
                Step2Circle.Source = "ic_fill_circle.png";
                Line1.Source = "ic_solid_line.png";
                break;

            case OrderProcessStep.OutForDelivery:
                Step1Circle.Source = "ic_fill_circle.png";
                Step2Circle.Source = "ic_fill_circle.png";
                Step3Circle.Source = "ic_fill_circle.png";
                Line1.Source = "ic_solid_line.png";
                Line2.Source = "ic_solid_line.png";
                break;

            case OrderProcessStep.Delivered:
                Step1Circle.Source = "ic_fill_circle.png";
                Step2Circle.Source = "ic_fill_circle.png";
                Step3Circle.Source = "ic_fill_circle.png";
                Step4Circle.Source = "ic_fill_circle.png";
                Line1.Source = "ic_solid_line.png";
                Line2.Source = "ic_solid_line.png";
                Line3.Source = "ic_solid_line.png";
                break;
        }
    }

    private void UpdateExpandState()
    {
        ProgressContainer.IsVisible = IsExpanded;
        ToggleImage.Source = IsExpanded ? "ic_arrow_up.png" : "ic_arrow_down.png";
    }

    private void OnToggleTapped(object sender, TappedEventArgs e)
    {
        IsExpanded = !IsExpanded;
    }
}