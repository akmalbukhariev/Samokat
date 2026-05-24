using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Samokat.Models.Startup;

public class OnboardingSlide : INotifyPropertyChanged
{
    private bool _showStartButton;

    public string Title { get; set; } = string.Empty;
    public string BigText { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;

    public bool ShowStartButton
    {
        get => _showStartButton;
        set
        {
            if (_showStartButton == value)
                return;

            _showStartButton = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}