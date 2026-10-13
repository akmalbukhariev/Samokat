using NinimumDelivery.Models;
using NinimumDelivery.Resources.Languages;
namespace NinimumDelivery.Views;

public sealed class PartialReturnPage : ContentPage
{
    private readonly TaskCompletionSource<List<ReturnQuantity>?> completion = new();
    private readonly List<(long Id, int Maximum, Entry Input)> quantities = new();
    private bool closing;
    private static string Text(string key) => AppResource.ResourceManager.GetString(key,AppResource.Culture) ?? key;
    public PartialReturnPage(IEnumerable<DeliveryProduct> products)
    {
        var body=new VerticalStackLayout { Padding=24, Spacing=18 };
        body.Add(new Label { Text=Text("PartialReturnTitle"), FontSize=24, FontAttributes=FontAttributes.Bold });
        body.Add(new Label { Text=Text("PartialReturnHint") });
        foreach(var group in products.GroupBy(p=>p.productId))
        {
            int maximum=group.Sum(p=>p.quantity);
            var input=new Entry { Text="0", Keyboard=Keyboard.Numeric, Placeholder="0" };
            quantities.Add((group.Key,maximum,input));
            body.Add(new Label { Text=$"{group.First().productName} · {maximum}", FontAttributes=FontAttributes.Bold });
            body.Add(new Label { Text=Text("PartialReturnQuantity") });body.Add(input);
        }
        var confirm=new Button { Text=Text("PartialReturnContinue"), BackgroundColor=Color.FromArgb("#FF3945"), TextColor=Colors.White };
        confirm.Clicked+=async (_,_)=>
        {
            var selected=new List<ReturnQuantity>();
            foreach(var row in quantities)
            {
                if(!int.TryParse(row.Input.Text,out int quantity) || quantity<0 || quantity>row.Maximum)
                { await DisplayAlertAsync(AppResource.Error,Text("PARTIAL_RETURN_INVALID"),AppResource.Close);return; }
                if(quantity>0) selected.Add(new ReturnQuantity { productId=row.Id,quantity=quantity });
            }
            if(selected.Count==0 || selected.Sum(p=>(long)p.quantity)>=quantities.Sum(p=>(long)p.Maximum))
            { await DisplayAlertAsync(AppResource.Error,Text("PARTIAL_RETURN_INVALID"),AppResource.Close);return; }
            await Close(selected);
        };
        var cancel=new Button { Text=AppResource.Close };cancel.Clicked+=async (_,_)=>await Close(null);
        body.Add(confirm);body.Add(cancel);Content=new ScrollView { Content=body };
    }
    private async Task Close(List<ReturnQuantity>? value)
    {
        if(closing)return;closing=true;
        await Navigation.PopModalAsync();completion.TrySetResult(value);
    }
    protected override void OnDisappearing()
    { base.OnDisappearing();if(!closing)completion.TrySetResult(null); }
    public static async Task<List<ReturnQuantity>?> Select(Page parent,IEnumerable<DeliveryProduct> products)
    {
        var page=new PartialReturnPage(products);await parent.Navigation.PushModalAsync(page);
        return await page.completion.Task;
    }
}
