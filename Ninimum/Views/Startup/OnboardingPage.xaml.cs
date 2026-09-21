using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;
using Ninimum.Models.Startup;
using Ninimum.Services;
using Ninimum.Resources.Languages;
using Utils;

namespace Ninimum.Views.Startup;

public partial class OnboardingPage : BasePage
{
    public ICommand StartCommand { get; }

    private readonly AppStoreService appStoreService;
    private readonly AppControl appControl;
    private bool isCompleting;

    private readonly List<OnboardingSlide> _slides = new()
    {
        new OnboardingSlide
        {
            Title = AppResource.OnboardingDiscountTitle,
            BigText = "15%",
            Description = AppResource.OnboardingDiscountDescription,
            Image = "onboarding_discount.png"
        },
        new OnboardingSlide
        {
            Title = AppResource.OnboardingOfferTitle,
            BigText = "50%",
            Description = AppResource.OnboardingOfferDescription,
            Image = "onboarding_offer.png"
        },
        new OnboardingSlide
        {
            Title = AppResource.OnboardingBabyTitle,
            BigText = AppResource.OnboardingBabyBigText,
            Description = AppResource.OnboardingBabyDescription,
            Image = "onboarding_baby.png"
        },
        new OnboardingSlide
        {
            Title = AppResource.OnboardingOnlineTitle,
            BigText = AppResource.OnboardingOnlineBigText,
            Description = AppResource.OnboardingOnlineDescription,
            Image = "onboarding_online.png"
        }
    };
    
    public OnboardingPage(AppStoreService appStoreService, AppControl appControl)
    {
        InitializeComponent();

        this.appStoreService = appStoreService;
        this.appControl = appControl;

        StartCommand = new Command(async () => await CompleteOnboardingAsync());

        OnboardingCarousel.ItemsSource = _slides;

        UpdateBottomSection(0);
        BindingContext = this;
    }

    private async void OnSkipTapped(object sender, TappedEventArgs e)
    {
        await AnimateElementScaleDown(lbSkip);
        await CompleteOnboardingAsync();
    }

    private void OnCarouselPositionChanged(object sender, PositionChangedEventArgs e)
    {
        UpdateBottomSection(e.CurrentPosition);
    }

    private async Task CompleteOnboardingAsync()
    {
        if (isCompleting)
            return;

        isCompleting = true;
        appStoreService.Set(AppKeys.HasCompletedOnboarding, true);

        try
        {
            // Do not create a second AppEntryShell from inside the first one.
            // The normal first-start path ends in guest mode, so enter it directly.
            await appControl.StartGuestMode();
        }
        catch
        {
            isCompleting = false;
            throw;
        }
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