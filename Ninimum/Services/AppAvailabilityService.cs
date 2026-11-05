using Models.Responses;
using Newtonsoft.Json;
using Ninimum.Models;
using RestSharp;
using Utils;

namespace Ninimum.Services;

/// <summary>Checks on startup/resume; an unavailable config endpoint never invents a maintenance lock.</summary>
public sealed class AppAvailabilityService(RestClient client)
{
    private readonly SemaphoreSlim gate=new(1,1);
    private bool optionalShown;

    // Google Play page of this app (Play Store app first, browser if it isn't installed).
    public static async Task OpenStoreAsync()
    {
        var id=AppInfo.Current.PackageName;
        try{if(await Launcher.TryOpenAsync(new Uri($"market://details?id={id}")))return;}catch{}
        await Launcher.OpenAsync(new Uri($"https://play.google.com/store/apps/details?id={id}"));
    }
    public async Task EnsureAvailableAsync()
    {
        if(!await gate.WaitAsync(0))return;
        try
        {
            var config=await ReadAsync();
            var state=AppAvailability.Evaluate(config,AppInfo.Current.VersionString);
            if(state==Availability.UpdateOptional)
            {
                // Once per app start: the customer may keep using the old version.
                if(optionalShown)return;
                optionalShown=true;
                var current=Application.Current?.Windows.FirstOrDefault()?.Page;
                if(current==null)return;
                bool update=await MainThread.InvokeOnMainThreadAsync(()=>current.DisplayAlert(
                    L("Yangi versiya mavjud","Доступна новая версия","A new version is available"),
                    L($"Ninimum ilovasining {config?.appVersion} versiyasi chiqdi. Yangilaysizmi?",$"Вышла версия Ninimum {config?.appVersion}. Обновить?",$"Ninimum {config?.appVersion} is out. Update now?"),
                    L("Yangilash","Обновить","Update"),L("Keyinroq","Позже","Later")));
                if(update)await OpenStoreAsync();
                return;
            }
            if(state==Availability.Available)return;
            var host=Application.Current?.Windows.FirstOrDefault()?.Page;
            if(host==null)return;
            var page=new AvailabilityPage();
            await MainThread.InvokeOnMainThreadAsync(()=>host.Navigation.PushModalAsync(page,false));
            try
            {
                // Keep the existing session/page intact behind the modal. Retry never replays a purchase.
                while(state!=Availability.Available)
                {
                    await MainThread.InvokeOnMainThreadAsync(()=>page.Show(state,config?.maintenanceMessage,config?.appVersion));
                    await page.WaitForRetryAsync();
                    var latest=await ReadAsync();
                    if(latest!=null)config=latest;
                    state=AppAvailability.Evaluate(config,AppInfo.Current.VersionString);
                }
            }
            finally
            {
                await MainThread.InvokeOnMainThreadAsync(async()=>
                {
                    if(host.Navigation.ModalStack.LastOrDefault()==page)await host.Navigation.PopModalAsync(false);
                });
            }
        }
        catch(Exception ex){System.Diagnostics.Debug.WriteLine($"Availability check: {ex.GetType().Name}");}
        finally{gate.Release();}
    }
    private async Task<ApplicationConfiguration?> ReadAsync()
    {
        try
        {
            using var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(6));
            var response=await client.ExecuteAsync(new RestRequest("appConfig/getAppConfig",Method.Get),timeout.Token);
            if(!response.IsSuccessful)return null;
            var result=JsonConvert.DeserializeObject<Response<ApplicationConfiguration>>(response.Content??"");
            return result?.resultCode=="100"?result.resultData:null;
        }
        catch{return null;}
    }
    private static string L(string uz,string ru,string en)=>AppService.GetRequired<LanguageService>().GetCurrentLanguage() switch{"ru"=>ru,"en"=>en,_=>uz};

    private sealed class AvailabilityPage : ContentPage
    {
        private readonly Label heading=new(){FontSize=24,FontAttributes=FontAttributes.Bold};
        private readonly Label message=new(){FontSize=17};
        private readonly Button retry=new();
        private readonly Button store=new(){BackgroundColor=Color.FromArgb("#FF4A3F"),TextColor=Colors.White};
        private TaskCompletionSource retrySource=new(TaskCreationOptions.RunContinuationsAsynchronously);
        public AvailabilityPage()
        {
            BackgroundColor=Colors.White;
            Shell.SetNavBarIsVisible(this,false);NavigationPage.SetHasNavigationBar(this,false);
            Content=new VerticalStackLayout{Padding=28,Spacing=20,VerticalOptions=LayoutOptions.Center,Children={heading,message,store,retry}};
            retry.Clicked+=(_,_)=>{retry.IsEnabled=false;retrySource.TrySetResult();};
            store.Clicked+=async(_,_)=>await OpenStoreAsync();
        }
        protected override bool OnBackButtonPressed()=>true;
        public void Show(Availability state,string? detail,string? version)
        {
            heading.Text=state==Availability.Maintenance?L("Texnik xizmat","Технические работы","Maintenance"):L("Ilovani yangilang","Обновите приложение","Update the app");
            message.Text=state==Availability.Maintenance
                ?(!string.IsNullOrWhiteSpace(detail)?detail:L("Iltimos, keyinroq qayta urinib ko‘ring.","Повторите попытку позже.","Please try again later."))
                :L($"Davom etish uchun {version} yoki yangi versiyani o‘rnating.",$"Для продолжения установите версию {version} или новее.",$"Install version {version} or newer to continue.");
            retry.Text=L("Qayta tekshirish","Проверить снова","Check again");
            store.Text=L("Yangilash","Обновить","Update");
            store.IsVisible=state==Availability.UpdateRequired;
            retrySource=new(TaskCreationOptions.RunContinuationsAsynchronously);retry.IsEnabled=true;
        }
        public Task WaitForRetryAsync()=>retrySource.Task;
    }
}
