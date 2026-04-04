
using System.Collections.ObjectModel;
using System.Windows.Input;
using Samokat.Models;
using Samokat.Models.Startup;

namespace Samokat.Views.Startup;

public partial class StartPage : BasePage
{
    public ObservableCollection<PopupItemModel> RegionItems { get; set; }
    public ObservableCollection<PopupItemModel> LanguageItems { get; set; }

    public ICommand OpenRegionPopupCommand { get; }
    public ICommand OpenLanguagePopupCommand { get; }

    public ICommand RegionSelectedCommand { get; }
    public ICommand LanguageSelectedCommand { get; }
    public ICommand ContinueCommand { get; }

    public StartPage()
    {
        InitializeComponent();

        OpenRegionPopupCommand = new Command(() => RegionPopup.IsVisible = true);
        OpenLanguagePopupCommand = new Command(() => LanguagePopup.IsVisible = true);

        RegionItems = new ObservableCollection<PopupItemModel>
        {
            new() { Text = "Qoraqalpog'iston" },
            new() { Text = "Andijon" },
            new() { Text = "Buxoro" },
            new() { Text = "Jizzax" },
            new() { Text = "Qashqadaryo", RightImage = "check_icon.png" },
            new() { Text = "Navoiy" },
            new() { Text = "Namangan" },
            new() { Text = "Samarqand" },
            new() { Text = "Sirdaryo" },
            new() { Text = "Surxondaryo" },
            new() { Text = "Farg'ona" },
            new() { Text = "Xorazm" },
            new() { Text = "Toshkent" }
        };

        LanguageItems = new ObservableCollection<PopupItemModel>
        {
            new() { Text = "O'zbekcha", LeftImage = "flag_uz.png", RightImage = "check_gray.png" },
            new() { Text = "Русский", LeftImage = "flag_ru.png" },
            new() { Text = "English", LeftImage = "flag_en.png" }
        };

        RegionSelectedCommand = new Command<PopupItemModel>(OnRegionSelected);
        LanguageSelectedCommand = new Command<PopupItemModel>(OnLanguageSelected);

        ContinueCommand = new Command(OnContinue);
        
        BindingContext = this;
    }

    private void OnRegionSelected(PopupItemModel item)
    {
        foreach (var region in RegionItems)
            region.RightImage = string.Empty;

        item.RightImage = "check_gray.png";

        RegionPopup.IsVisible = false;
    }

    private void OnLanguageSelected(PopupItemModel item)
    {
        foreach (var language in LanguageItems)
            language.RightImage = string.Empty;

        item.RightImage = "check_gray.png";

        LanguagePopup.IsVisible = false;
    }

    private void OnRegionTapped(object sender, TappedEventArgs e)
    {
        RegionPopup.IsVisible = true;
    }

    private void OnLanguageTapped(object sender, TappedEventArgs e)
    {
        LanguagePopup.IsVisible = true;
    }
    
    private async void OnContinue()
    {
        await AppNavigatorService.NavigateTo(nameof(OnboardingPage));
    }
}