using System.Collections.ObjectModel;
using Ninimum.Models;

namespace Ninimum.Views.Children;

public partial class ChildrenPage : BasePage
{
    public ObservableCollection<ChildrenInfo> ChildernList { get; set; }

    public ChildrenPage()
    {
        InitializeComponent();

        ChildernList = new ObservableCollection<ChildrenInfo>
        {
            new ChildrenInfo
            {
                FirstName = "Toshmat",
                LastName = "Eshmatov",
                BirthDate = "26.03.2000",
                Age = "5 yosh",
                Gender = "Erkak"
            },
            new ChildrenInfo
            {
                FirstName = "Toshmat",
                LastName = "Eshmatov",
                BirthDate = "26.03.2000",
                Age = "5 yosh",
                Gender = "Erkak"
            }
        };

        BindingContext = this;
    }
    
    private async void Add_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ChildInfoPage(ChildernList));
    }

    private async void Edit_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Image image && image.BindingContext is ChildrenInfo child)
        {
            await Navigation.PushAsync(new ChildInfoPage(ChildernList, child));
        }
    }

    private void Delete_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Image image && image.BindingContext is ChildrenInfo child)
        {
            ChildernList.Remove(child);
        }
    }
}