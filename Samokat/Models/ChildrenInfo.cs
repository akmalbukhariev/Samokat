using CommunityToolkit.Mvvm.ComponentModel;

namespace Samokat.Models;

public partial class ChildrenInfo : ObservableObject
{
    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string lastName = string.Empty;

    [ObservableProperty]
    private string birthDate = string.Empty;

    [ObservableProperty]
    private string age = string.Empty;

    [ObservableProperty]
    private string gender = string.Empty;
}