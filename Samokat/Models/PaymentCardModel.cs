using System.Windows.Input;

namespace Samokat.Models;

public class PaymentCardModel
{
    public string CardNumber { get; set; }
    public string ExpireDate { get; set; }
    public bool IsPrimary { get; set; }

    public ICommand ToggleCommand => new Command(() =>
    {
        IsPrimary = !IsPrimary;
    });

    public ICommand DeleteCommand => new Command(() =>
    {
        // handle delete
    });
}