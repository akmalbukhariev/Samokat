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
    public async Task EnsureAvailableAsync()
    {
        if(!await gate.WaitAsync(0))return;
        try
        {
            var config=await ReadAsync();
            var state=AppAvailability.Evaluate(config,AppInfo.Current.VersionString);
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
    private sealed class AvailabilityPage : ContentPage
    {
        private readonly Label heading=new(){FontSize=24,FontAttributes=FontAttributes.Bold};
        private readonly Label message=new(){FontSize=17};
        private readonly Button retry=new();
        private TaskCompletionSource retrySource=new(TaskCreationOptions.RunContinuationsAsynchronously);
        public AvailabilityPage()
        {
            BackgroundColor=Colors.White;
            Shell.SetNavBarIsVisible(this,false);NavigationPage.SetHasNavigationBar(this,false);
            Content=new VerticalStackLayout{Padding=28,Spacing=20,VerticalOptions=LayoutOptions.Center,Children={heading,message,retry}};
            retry.Clicked+=(_,_)=>{retry.IsEnabled=false;retrySource.TrySetResult();};
        }
        protected override bool OnBackButtonPressed()=>true;
        private static string L(string uz,string ru,string en)=>AppService.GetRequired<LanguageService>().GetCurrentLanguage() switch{"ru"=>ru,"en"=>en,_=>uz};
        public void Show(Availability state,string? detail,string? version)
        {
            heading.Text=state==Availability.Maintenance?L("Texnik xizmat","Технические работы","Maintenance"):L("Ilovani yangilang","Обновите приложение","Update the app");
            message.Text=state==Availability.Maintenance
                ?(!string.IsNullOrWhiteSpace(detail)?detail:L("Iltimos, keyinroq qayta urinib ko‘ring.","Повторите попытку позже.","Please try again later."))
                :L($"Davom etish uchun {version} yoki yangi versiyani o‘rnating.",$"Для продолжения установите версию {version} или новее.",$"Install version {version} or newer to continue.");
            retry.Text=L("Qayta tekshirish","Проверить снова","Check again");
            retrySource=new(TaskCreationOptions.RunContinuationsAsynchronously);retry.IsEnabled=true;
        }
        public Task WaitForRetryAsync()=>retrySource.Task;
    }
}
