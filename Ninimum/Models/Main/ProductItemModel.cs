using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ninimum.Models.Main;
 
public partial class ProductItemModel : ObservableObject
{
    public long CompanyId;
    public long PromotionId;
    public long BookmarkId = 0;
    public decimal? NewPriceDigit;
    public decimal? OldPriceDigit;
    public string description;
    [ObservableProperty] private string productImage;
    [ObservableProperty] private string newPrice;
    [ObservableProperty] private string oldPrice;
    [ObservableProperty] private string productName;
    [ObservableProperty] private string rating;
}
 
