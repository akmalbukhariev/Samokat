using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Api.Services;
using Models.Requests;
using Models.Responses;
using Ninimum.Services;
using Utils;

namespace Ninimum.Views.PaymentCard;

public partial class AddPaymentCardPage : BasePage, INotifyPropertyChanged
{
    private string _cardNumber = string.Empty;
    private string _expireDate = string.Empty;
    private string _cvv = string.Empty;
    private bool _rememberCard = true;
    private bool _isUpdatingExpireDate;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public string CardNumber
    {
        get => _cardNumber;
        set
        {
            var digits = new string((value ?? string.Empty)
                .Where(char.IsDigit)
                .ToArray());

            if (digits.Length > 16)
                digits = digits[..16];

            if (_cardNumber != digits)
            {
                _cardNumber = digits;
                OnPropertyChanged();
            }
        }
    }
    
    public string Cvv
    {
        get => _cvv;
        set
        {
            if (_cvv != value)
            {
                _cvv = value;
                OnPropertyChanged();
            }
        }
    }

    public bool RememberCard
    {
        get => _rememberCard;
        set
        {
            if (_rememberCard != value)
            {
                _rememberCard = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand ToggleRememberCommand { get; }

    private readonly UserApiService apiService;
    public AddPaymentCardPage(AppControl appControl, UserApiService apiService)
    {
        InitializeComponent();

        base.appControl = appControl;
        this.apiService = apiService;
        
        ToggleRememberCommand = new Command(() =>
        {
            RememberCard = !RememberCard;
        });

        BindingContext = this;
    }

    private string _expireMonth = string.Empty;
    public string ExpireMonth
    {
        get => _expireMonth;
        set
        {
            var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length > 2)
                digits = digits[..2];

            if (_expireMonth != digits)
            {
                _expireMonth = digits;
                OnPropertyChanged();
            }
        }
    }

    private string _expireYear = string.Empty;
    public string ExpireYear
    {
        get => _expireYear;
        set
        {
            var digits = new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length > 2)
                digits = digits[..2];

            if (_expireYear != digits)
            {
                _expireYear = digits;
                OnPropertyChanged();
            }
        }
    }

    public string ExpireDate => $"{ExpireMonth}/{ExpireYear}"; 

    protected override void OnAppearing()
    {
        base.OnAppearing();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(300);
            CardNumberEntry.Focus();
        });
    }
        
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void SaveCard_Tapped(object sender, TappedEventArgs e)
    {
        await AnimateElementScaleDown(sender as Border);

        try
        {
            string cleanCardNumber = new string(
                (CardNumber ?? string.Empty).Where(char.IsDigit).ToArray());

            if (cleanCardNumber.Length != 16)
            {
                await DisplayAlert("Xatolik", "Karta raqamini to‘liq kiriting.", "OK");
                return;
            }

            if (!int.TryParse(ExpireMonth, out int expiryMonth) ||  expiryMonth < 1 || expiryMonth > 12)
            {
                await DisplayAlert("Xatolik", "Amal qilish oyini to‘g‘ri kiriting.", "OK");
                return;
            }

            if (!int.TryParse(ExpireYear, out int expiryYear))
            {
                await DisplayAlert("Xatolik", "Amal qilish yilini to‘g‘ri kiriting.", "OK");
                return;
            }

            if (expiryYear < 100)
                expiryYear += 2000;

            loading.ShowLoading = true;
            Response response = await apiService.AddPaymentCard(
                new CreatePaymentCardRequest
                {
                    user_id = appControl.userDto.id ?? 0,

                    card_number = cleanCardNumber,

                    card_brand = string.Empty,
                    card_hash = string.Empty,
                    last_four_digits = string.Empty,
                    payment_token = string.Empty,

                    card_holder_name = string.Empty,
                    expiry_month = expiryMonth,
                    expiry_year = expiryYear,

                    cvv = string.Empty,
                    is_default = true
                });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                await DisplayAlert("Xatolik", response.resultMsg, "OK");
                return;
            }

            await DisplayAlert("Muvaffaqiyatli", "Karta saqlandi.", "OK");

            PaymentCardPage.NeedRefreshCards = true;
            await AppNavigatorService.NavigateTo("..");
        }
        catch
        {
            await DisplayAlert("Xatolik", "Kartani saqlab bo‘lmadi.", "OK");
        }
        finally
        { 
            loading.ShowLoading = false;
        }
    }
}