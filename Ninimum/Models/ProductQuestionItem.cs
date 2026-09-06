namespace Ninimum.Models;

public class ProductQuestionItem
{
    public long Id { get; set; }
    public string CustomerName { get; set; } = "Foydalanuvchi";
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;
    public string AnsweredDate { get; set; } = string.Empty;
    public bool IsAnswered { get; set; }
    public bool IsWaiting => !IsAnswered;
}
