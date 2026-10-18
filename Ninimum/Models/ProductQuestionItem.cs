using Ninimum.Resources.Languages;
namespace Ninimum.Models;

public class ProductQuestionItem
{
    public bool IsComplaint { get; set; }
    public string KindLabel => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch {"uz"=>"Shikoyat (faqat sizga ko‘rinadi)","ru"=>"Жалоба (видна только вам)",_=>"Complaint (visible only to you)"};
    public long Id { get; set; }
    public string CustomerName { get; set; } = AppResource.User;
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;
    public string AnsweredDate { get; set; } = string.Empty;
    public bool IsAnswered { get; set; }
    public bool IsWaiting => !IsAnswered;
}
