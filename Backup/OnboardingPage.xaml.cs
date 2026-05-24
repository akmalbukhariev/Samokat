using System.Windows.Input;
using Ninimum.Models.Startup;

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

        BindingContext = this;

        OnboardingCarousel.ItemsSource = _slides;
        UpdateBottomSection(0);
    }

    private async void OnSkipTapped(object sender, TappedEventArgs e)
    {
        await AnimateElementScaleDown(lbSkip);
        await Navigation.PushAsync(new StartPage());
    }

    private void OnCarouselPositionChanged(object sender, PositionChangedEventArgs e)
    {
        UpdateBottomSection(e.CurrentPosition);
    }

    private async void OnStart()
    {
        //await AnimateElementScaleDown(btnStart);
        await Navigation.PushAsync(new StartPage());
    }

    private void UpdateBottomSection(int position)
    {
        if (position < 0 || position >= _slides.Count) return;
        for (int i = 0; i < _slides.Count; i++)
        {
            _slides[i].ShowStartButton = i == position && i == _slides.Count - 1;
        }
    }
}