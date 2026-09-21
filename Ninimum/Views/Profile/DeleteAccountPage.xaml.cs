using Ninimum.Resources.Languages;
using Api.Services;
using Models.Requests;
using Ninimum.Services;
using Utils;

namespace Ninimum.Views.Profile;

public partial class DeleteAccountPage : BasePage
{
    private readonly UserApiService apiService;

    public DeleteAccountPage(AppControl appControl, UserApiService apiService)
    {
        InitializeComponent();

        this.appControl = appControl;
        this.apiService = apiService;

        EnableTap(RowTooExpensive, CbTooExpensive);
        EnableTap(RowNotEnoughValue, CbNotEnoughValue);
        EnableTap(RowNotEnoughOffers, CbNotEnoughOffers);
        EnableTap(RowHardToUse, CbHardToUse);
        EnableTap(RowNoTime, CbNoTime);
        EnableTap(RowPreferOther, CbPreferOther);
        EnableTap(RowTechnicalIssues, CbTechnicalIssues);
        EnableTap(RowOther, CbOther);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!await appControl.EnsureAuthenticatedAsync(true))
            return;
    }

    private string BuildReasonsText()
    {
        var parts = new List<string>();

        void Add(bool condition, string code)
        {
            if (condition)
                parts.Add(code);
        }

        Add(CbTooExpensive.IsChecked, "TE");
        Add(CbNotEnoughValue.IsChecked, "NEV");
        Add(CbNotEnoughOffers.IsChecked, "NEO");
        Add(CbHardToUse.IsChecked, "HTU");
        Add(CbNoTime.IsChecked, "NT");
        Add(CbPreferOther.IsChecked, "PO");
        Add(CbTechnicalIssues.IsChecked, "TI");

        if (CbOther.IsChecked)
        {
            string other = (OtherEntry.Text ?? string.Empty).Trim();
            other = other.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

            if (other.Length > 80)
                other = other[..80];

            parts.Add(string.IsNullOrWhiteSpace(other) ? "O" : $"O:{other}");
        }

        return string.Join("|", parts);
    }

    private static void EnableTap(Grid row, CheckBox checkBox)
    {
        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, _) => checkBox.IsChecked = !checkBox.IsChecked;
        row.GestureRecognizers.Add(tap);
    }

    private void CbOther_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        OtherEntry.IsVisible = e.Value;
        OtherEntryBackground.IsVisible = e.Value;

        if (!e.Value)
            OtherEntry.Text = string.Empty;
    }

    private async void DeleteButton_Clicked(object sender, EventArgs e)
    {
        if (sender is not VisualElement element)
            return;

        await ClickGuard.RunAsync(element, async () =>
        {
            string reasons = BuildReasonsText();

            if (string.IsNullOrWhiteSpace(reasons))
            {
                await DisplayAlert(
                    AppResource.SelectReason,
                    AppResource.SelectAtLeastOneReasonForDeletingThe,
                    AppResource.Ok);
                return;
            }

            if (CbOther.IsChecked && string.IsNullOrWhiteSpace(OtherEntry.Text))
            {
                await DisplayAlert(
                    AppResource.EnterReasonTitle,
                    AppResource.OtherReasonIsSelectedPleaseBrieflyDescribeThe,
                    AppResource.Ok);
                return;
            }

            bool confirmed = await DisplayAlert(
                AppResource.DeleteAccount,
                AppResource.AfterTheAccountIsDeletedItCannotBe,
                AppResource.Delete,
                AppResource.Cancel);

            if (!confirmed)
                return;

            try
            {
                loading.ShowLoading = true;

                var response = await apiService.DeleteUserAccount(new DeleteAccountRequest
                {
                    userId = appControl.CurrentUserId,
                    reasons = reasons
                });

                loading.ShowLoading = false;

                if (response.resultCode != ApiResult.SUCCESS.GetCodeToString())
                {
                    await DisplayAlert(
                        AppResource.Error,
                        response.resultMsg ?? AppResource.CouldNotDeleteTheAccountPleaseTryAgain,
                        AppResource.Close);
                    return;
                }

                await DisplayAlert(
                    AppResource.AccountDeleted,
                    AppResource.ThankYouForYourFeedbackYourAccountWas,
                    AppResource.Ok);

                await appControl.StartGuestMode(clearSavedLogin: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] DeleteAccount => {ex}");
                loading.ShowLoading = false;

                await DisplayAlert(
                    AppResource.Error,
                    AppResource.CouldNotDeleteTheAccountPleaseTryAgain,
                    AppResource.Close);
            }
            finally
            {
                loading.ShowLoading = false;
            }
        });
    }
}
