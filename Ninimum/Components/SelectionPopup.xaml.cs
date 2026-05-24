using CommunityToolkit.Maui.Views;
using System.Collections;
using Ninimum.Models;
using Ninimum.Models.Startup;

namespace Ninimum.Components;

public partial class SelectionPopup : Popup
{
    public IEnumerable? Items { get; set; }

    public SelectionPopup(IEnumerable items, double popupWidth = 320, double popupMaxHeight = 620)
    {
        InitializeComponent();

        Items = items;
        BindingContext = this;

        PopupBorder.WidthRequest = popupWidth;
        PopupScroll.MaximumHeightRequest = popupMaxHeight;
    }

    private void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if (sender is TapGestureRecognizer tap &&
            tap.CommandParameter is PopupItemModel item)
        {
            Close(item);
            return;
        }

        if (e.Parameter is PopupItemModel item2)
        {
            Close(item2);
        }
    }
}