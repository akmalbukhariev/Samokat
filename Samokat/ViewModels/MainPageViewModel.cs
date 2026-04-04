using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Samokat.ViewModels;

public partial class MainPageViewModel : ObservableObject
{  
    [ObservableProperty] private ICommand notificationTapCommand;
    [ObservableProperty] private ICommand buyNowCommand;
    [ObservableProperty] private ICommand menuCommand;

    public MainPageViewModel()
    {
        NotificationTapCommand = new Command(OnNotificationTapped);
    }
    
    private async void OnNotificationTapped()
    {
        await Application.Current.MainPage.DisplayAlert("Info", "Notification clicked", "OK");
    }
}