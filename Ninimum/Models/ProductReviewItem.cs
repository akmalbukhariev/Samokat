using System.Collections.ObjectModel;

namespace Ninimum.Models;

public class ProductReviewItem
{
    public string CustomerName { get; set; } = string.Empty;
    public string ReviewDate { get; set; } = string.Empty;
    public DateTime ReviewDateValue { get; set; }
    public int Rating { get; set; }
    public string ReviewText { get; set; } = string.Empty;
    public string ReplyText { get; set; } = string.Empty;
    public ObservableCollection<string> Photos { get; set; } = new();

    public bool HasPhotos => Photos != null && Photos.Count > 0;
    public bool HasReply => !string.IsNullOrWhiteSpace(ReplyText);

    public string StarsText
    {
        get
        {
            var full = new string('★', Rating);
            var empty = new string('☆', 5 - Rating);
            return full + empty;
        }
    }
}