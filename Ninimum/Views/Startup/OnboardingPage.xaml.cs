using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;
using Ninimum.Models.Startup;
using Ninimum.Views.LoginRegister;

namespace Ninimum.Views.Startup;

public partial class OnboardingPage : BasePage
{
    public ICommand StartCommand { get; }

    private readonly List<OnboardingSlide> _slides = new()
    {
        new OnboardingSlide
        {
            Title = "Doimiy\nchegirmalar\nsizni kutmoqda",
            BigText = "15%",
            Description = "Chegirmalarni doimiy ravishda dastur orqali kuzatib borishingiz mumkin.",
            Image = "onboarding_discount.png"
        },
        new OnboardingSlide
        {
            Title = "Ilovadan\nfoydalanuvchilari\nuchun maxsus\nchegirmaviy\ntakliflar",
            BigText = "50%",
            Description = "Ilovadan foydalanuvchilari uchun maxsus chegirmaviy takliflar.",
            Image = "onboarding_offer.png"
        },
        new OnboardingSlide
        {
            Title = "Bolalar uchun eng\nkerakli\nmahsulotlarni\nbizning ilova\norqali toping",
            BigText = "Baby",
            Description = "Bolalar uchun eng kerakli mahsulotlarni bizning ilova orqali toping.",
            Image = "onboarding_baby.png"
        },
        new OnboardingSlide
        {
            Title = "Xonadoningiz\nuchun zarur bo‘lgan\nmahsulotlarni\nonlayn tarzda\nbuyurtma qiling",
            BigText = "Online",
            Description = "Xonadoningiz uchun zarur bo‘lgan mahsulotlarni onlayn tarzda buyurtma qiling.",
            Image = "onboarding_online.png"
        }
    };
    
    public OnboardingPage()
    {
        InitializeComponent();

        StartCommand = new Command(OnStart);

        OnboardingCarousel.ItemsSource = _slides;

        UpdateBottomSection(0);
        BindingContext = this;
    }

    private async void OnSkipTapped(object sender, TappedEventArgs e)
    {
        await AnimateElementScaleDown(lbSkip);
        await AppNavigatorService.NavigateTo(nameof(LoginPage));
    }

    private void OnCarouselPositionChanged(object sender, PositionChangedEventArgs e)
    {
        UpdateBottomSection(e.CurrentPosition);
    }

    private async void OnStart()
    {
        await AppNavigatorService.NavigateTo(nameof(LoginPage));
    }

    private void UpdateBottomSection(int position)
    {
        if (position < 0 || position >= _slides.Count)
            return;

        lbDescription.Text = _slides[position].Description;
        btnStart.IsVisible = position == _slides.Count - 1;

        UpdateCustomIndicator(position);
    }

    private void UpdateCustomIndicator(int position)
    {
        CustomIndicatorLayout.Children.Clear();

        for (int i = 0; i < _slides.Count; i++)
        {
            bool isSelected = i == position;

            var indicator = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = isSelected
                    ? Color.FromArgb("#FF5B52")
                    : Color.FromArgb("#D0D0D0"),
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