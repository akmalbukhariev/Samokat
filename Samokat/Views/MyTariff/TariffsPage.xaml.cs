using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;
using Samokat.Models.Tariff;

namespace Samokat.Views.MyTariff;

public partial class TariffsPage : BasePage
{
    public ObservableCollection<TariffPlan> Tariffs { get; set; }

    public TariffsPage()
    {
        InitializeComponent();

        Tariffs = new ObservableCollection<TariffPlan>
        {
            new TariffPlan
            {
                Name = "Silver",
                Price = "49 000",
                Color = Color.FromArgb("#76B900"),
                DeliveryText = "Mahsulotni kun davomida yetkazish",
                PartnerIcon = "ic_uncheck_circle.png"
            },
            new TariffPlan
            {
                Name = "Gold",
                Price = "99 000",
                Color = Color.FromArgb("#FF8700"),
                DeliveryText = "Mahsulotni 3 soat davomida yetkazish",
                PartnerIcon = "ic_check_circle.png"
            },
            new TariffPlan
            {
                Name = "Platinum",
                Price = "199 000",
                Color = Color.FromArgb("#FF403B"),
                DeliveryText = "Mahsulotni 1soat davomida yetkazish",
                PartnerIcon = "ic_check_circle.png"
            }
        };

        BindingContext = this;
        UpdateCustomIndicator(0);
    }

    private void OnTariffPositionChanged(object sender, PositionChangedEventArgs e)
    {
        UpdateCustomIndicator(e.CurrentPosition);
    }

    private void UpdateCustomIndicator(int position)
    {
        CustomIndicatorLayout.Children.Clear();

        for (int i = 0; i < Tariffs.Count; i++)
        {
            bool isSelected = i == position;

            var indicator = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = isSelected
                    ? Color.FromArgb("#FF4B4B")
                    : Color.FromArgb("#D8D8D8"),
                WidthRequest = isSelected ? 36 : 12,
                HeightRequest = 12,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(6)
                },
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            CustomIndicatorLayout.Children.Add(indicator);
        }
    }
}