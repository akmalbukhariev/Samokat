using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Models
{
    public partial class ChildInputModel : ObservableObject
    {
        [ObservableProperty]
        private string firstName = string.Empty;

        [ObservableProperty]
        private string lastName = string.Empty;

        [ObservableProperty]
        private string birthDate = string.Empty;

        [ObservableProperty]
        private bool isBoySelected = true;

        [ObservableProperty]
        private bool isGirlSelected = false;

        [RelayCommand]
        private void SelectGender(string gender)
        {
            if (gender == "boy")
            {
                IsBoySelected = true;
                IsGirlSelected = false;
            }
            else
            {
                IsBoySelected = false;
                IsGirlSelected = true;
            }
        }
        
        [ObservableProperty]
        private DateTime selectedBirthDate = DateTime.Today;

        partial void OnSelectedBirthDateChanged(DateTime value)
        {
            BirthDate = value.ToString("yyyy-MM-dd");
        }
    }
 }