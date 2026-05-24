
using System.Collections.ObjectModel;
using System.Windows.Input;
using Ninimum.Models;
using Ninimum.Models.Startup;
using Ninimum.Views.LoginRegister;

namespace Ninimum.Views.Startup;

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


/*
Start Page:
- Logo
- App Name (Ninimum.uz)
- Region Selection
- Language Selection
- Continue Button
- App Version
- Region Popup List
- Language Popup List


Onboarding Page:
- Skip Button
- Onboarding Carousel
- Slide Image
- Slide Title
- Slide Big Text
- Slide Description
- Carousel Indicator
- Start Button


Login Page:
- Logo
- App Name
- Phone Number Input
- Password Input
- Password Show/Hide Button
- Forgot Password Button
- Login Button
- Register Button
- Guest Login Button
- SMS Code Popup
- SMS Code Confirmation


Register Page:
- Back Button
- Registration Information Text
- First Name Input
- Last Name Input
- Phone Number Input
- Region Input
- Password Input
- Confirm Password Input
- Child Information Section
- Child Gender Selection
- Child First Name Input
- Child Last Name Input
- Child Birth Date Input
- Add Child Button
- Agreement Checkbox
- Register Button
- Warning Information Text


Forgot Password Page:
- Back Button
- Password Recovery Description
- Phone Number Input
- Send Button
- Warning Information Text

Main Page:
- Main Header
- Search Bar
- Order Process Status
- Advertisement Banner
- Product List
- Product Images
- Product Title
- Product Old Price
- Product New Price
- Product Rating
- Product Review Count
- Product Action Button
- Product Detail Navigation
- Tomorrow Delivery Button
- Sticky Search Bar


Favorite Page:
- Page Header
- Favorite Product List
- Product Images
- Product Title
- Product Old Price
- Product New Price
- Product Rating
- Product Review Count
- Product Action Button


Detail Product Page:
- Back Button
- Product Image Carousel
- Product Image Preview
- Product Thumbnail List
- Favorite Button
- Product Title
- Stock Information
- Product Rating
- Product Review Count
- Product Comment Section
- Subscription Price Section
- Regular Price Section
- Delivery Information
- Product Question Button
- Product Description
- View Full Description Button
- Similar Products List
- Product Quantity Counter
- Minus Quantity Button
- Plus Quantity Button
- Buy Now Button
- Add To Cart Button
- Full Screen Image Preview


Leave Comment Page:
- Page Header
- Product Information
- Product Rating
- Rating Stars
- Comment Input
- Photo Upload Section
- Uploaded Photo Preview
- Add Photo Button
- Submit Comment Button


Product Reviews Page:
- Back Button
- Filter Button
- Buyer Photos Section
- Buyer Photos List
- Product Information
- Reviews List
- Customer Name
- Review Date
- Review Rating
- Review Text
- Review Photos
- Admin Reply
- Product Quantity Counter
- Minus Quantity Button
- Plus Quantity Button
- Buy Now Button
- Add To Cart Button
- Reviews Filter Bottom Sheet


Child Info Page:
- Page Header
- Child Gender Selection
- Child First Name Input
- Child Last Name Input
- Child Birth Date Input
- Agreement Confirmation
- Warning Information Text
- Save Button


Children Page:
- Page Header
- Children List
- Child First Name
- Child Last Name
- Child Birth Date
- Child Age
- Child Gender
- Edit Child Button
- Delete Child Button
- Add Child Button


Change Phone Number Page:
- Page Header
- Current Phone Number
- New Phone Number Input
- Password Input
- Password Show/Hide Button
- Warning Information Text
- Agreement Checkbox
- Change Button


Change Password Page:
- Page Header
- Current Password Input
- New Password Input
- Confirm New Password Input
- Password Show/Hide Button
- Change Password Button


Basket Product Page:
- Page Header
- Basket Product Count
- Delete Selected Products Button
- Select All Products Button
- Basket Product List
- Product Image
- Product Title
- Product Old Price
- Product New Price
- Product Quantity
- Product Selection Checkbox
- Quantity Change Button
- Basket Summary Section
- Total Selected Price
- Tariff Information
- Join Tariff Button
- Total Basket Price
- Total Basket Product Count
- Checkout Button


Formalization Page:
- Page Header
- Delivery Method Selection
- Pickup Delivery Option
- Courier Delivery Option
- Delivery Address
- Delivery Information
- Change Address Button
- Ordered Products List
- Payment Method Selection
- Online Payment Option
- Cash Payment Option
- Saved Bank Cards List
- Select Bank Card
- Add New Bank Card Button
- Order Summary Section
- Products Count
- Products Total Price
- Delivery Price
- Total Payment Price
- Privacy Agreement Information
- Complete Order Button


My Tariff Page:
- Page Header
- Current Tariff Information
- Tariff Plan Name
- Tariff Start Date
- Tariff Remaining Days
- Tariff Price
- Next Payment Date
- Tariff History Section
- Tariff History Toggle Button
- Tariff History List
- Previous Tariff Plan
- Previous Tariff Start Date
- Previous Tariff End Date
- Previous Tariff Price


Tariffs Page:
- Page Header
- Tariff Carousel
- Tariff Plan Name
- Tariff Monthly Price
- Free Delivery Information
- Delivery Discount Information
- Partner Discount Information
- Partner Service Information
- Purchase Tariff Button
- Tariff Carousel Indicator
- Tariff Information Note


Add Payment Card Page:
- Page Header
- Card Number Input
- Card Expire Month Input
- Card Expire Year Input
- Card CVV Input
- Remember Card Option
- Toggle Remember Card Button
- Card Information Warning Text
- Save Card Button


Cancel Order Page:
- Page Header
- Order Information
- Product Name
- Order Number
- Order Date
- Order Amount
- Cancel Reason Input
- Cancel Order Button
- Do Not Cancel Button
- Cancel Confirmation Popup
- Cancel Success Message


Payment Card Page:
- Page Header
- Saved Cards List
- Primary Card Toggle
- Card Number
- Card Expire Date
- Delete Card Button
- Add New Card Button



My Profile Page:
- User Full Name
- User Phone Number
- Account Verification Status
- Orders Menu
- Reviews Menu
- Payment Cards Menu
- Messages Menu
- Notifications Menu
- Settings Section
- Region Setting
- Language Setting
- Theme Setting
- Change Phone Number Menu
- Change Password Menu
- My Tariff Menu
- Children Menu
- Delete Account Button
- Logout Button
- Logout Confirmation Popup



Search Page:
- Page Header
- Search Input
- Filter Button
- Product Search Results
- Product Images
- Product Title
- Product Old Price
- Product New Price
- Product Rating
- Product Review Count
- Product Action Button
- Search Filter Bottom Sheet


*/