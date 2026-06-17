 
using Api.Services;
using Models.Requests;
using Models.Responses;
using Ninimum.Models;
using Ninimum.Models.Dto;
using Ninimum.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Utils;

namespace Ninimum.Views.PaymentCard;

public partial class PaymentCardPage : BasePage
{
    public static List<PaymentCardDto> NavigationCards { get; set; } = new();

    public ObservableCollection<PaymentCardModel> Cards { get; set; } = new();

    public ICommand TogglePrimaryCommand { get; }
    public ICommand DeleteCardCommand { get; }
    public ICommand AddCardCommand { get; }

    private readonly UserApiService apiService;
    private readonly AppControl appControl;
    public PaymentCardPage(AppControl appControl, UserApiService apiService)
    {
        InitializeComponent();

        this.appControl = appControl;
        this.apiService = apiService;

        TogglePrimaryCommand = new Command<PaymentCardModel>(async card => await OnTogglePrimary(card));
        DeleteCardCommand = new Command<PaymentCardModel>(async card => await OnDeleteCard(card));
        AddCardCommand = new Command(async () => await OnAddCard());

        BindingContext = this;

        LoadCards();
    }

    private void LoadCards()
    {
        Cards.Clear();

        foreach (var card in NavigationCards)
        {
            Cards.Add(new PaymentCardModel
            {
                Id = card.id,
                CardNumber = $"**** **** **** {card.last_four_digits}",
                ExpireDate = $"{card.expiry_month:D2}/{card.expiry_year % 100:D2}",
                IsPrimary = card.is_default
            });
        }
    }
 
    private async Task OnTogglePrimary(PaymentCardModel selectedCard)
    {
        if (selectedCard == null)
            return;

        // Save current state
        var previousPrimary = Cards.FirstOrDefault(x => x.IsPrimary);

        try
        {
            // Update UI immediately
            foreach (var card in Cards)
            {
                card.IsPrimary = card.Id == selectedCard.Id;
            }

            loading.ShowLoading = true;
            Response response = await apiService.SetDefaultPaymentCard(
                new SetDefaultPaymentCardRequest()
                {
                    card_id = (int)selectedCard.Id,
                    user_id = appControl.userDto.id ?? 0
                });

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                // Restore previous state
                foreach (var card in Cards)
                {
                    card.IsPrimary = previousPrimary != null &&
                                     card.Id == previousPrimary.Id;
                }

                await AlertService.ShowAlertAsync(
                    "Karta",
                    "Asosiy karta o‘zgartirilmadi");

                return;
            }

            // Update navigation cache too
            foreach (var dto in NavigationCards)
            {
                dto.is_default = dto.id == selectedCard.Id;
            }
        }
        catch (Exception ex)
        {
            // Restore previous state
            foreach (var card in Cards)
            {
                card.IsPrimary = previousPrimary != null &&
                                 card.Id == previousPrimary.Id;
            }

            await AlertService.ShowAlertAsync(
                "Xatolik",
                "Asosiy kartani o‘zgartirishda xatolik yuz berdi");
        }
        finally
        {
            loading.ShowLoading = false;
        }
    }

    private async void OnDeleteCardTapped(object sender, TappedEventArgs e)
    {
        if (sender is not VisualElement element)
            return;

        if (element.BindingContext is not PaymentCardModel card)
            return;

        await AnimateElementScaleDown(element);
        await OnDeleteCard(card);
    }

    private async Task OnDeleteCard(PaymentCardModel card)
    {
        try
        {
            if (card == null)
                return;

            bool confirm = await DisplayAlert(
                "Kartani o‘chirish",
                $"{card.CardNumber} kartasini o‘chirmoqchimisiz?",
                "Ha",
                "Yo‘q");

            if (!confirm)
                return;

            loading.ShowLoading = true;

            Response response = await apiService.DeletePaymentCard(
                new DeletePaymentCardRequest()
                {
                    card_id = (int)card.Id,
                    user_id = appControl.userDto.id ?? 0
                }
            );

            if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
            {
                await AlertService.ShowAlertAsync("Karta", "Karta ma’lumotlari o'chirilmadi");
                return;
            }

            Cards.Remove(card);

            var dto = NavigationCards.FirstOrDefault(x => x.id == card.Id);
            if (dto != null)
                NavigationCards.Remove(dto);

            if (Cards.Count > 0 && !Cards.Any(x => x.IsPrimary))
            {
                Cards[0].IsPrimary = true;
            }
        }
        finally
        {
            loading.ShowLoading = false;
        }
    }

    private async Task OnAddCard()
    {
        await AnimateElementScaleDown(frAddCard);
        await AppNavigatorService.NavigateTo(nameof(AddPaymentCardPage));
    }
}