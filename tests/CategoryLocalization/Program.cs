using Ninimum.Models;
var cases = new (string? Fallback,string? Uz,string? Ru,string Language,string Expected)[] {
    ("Milk","Sut","Молоко","uz","Sut"),
    ("Milk","Sut","Молоко","ru","Молоко"),
    ("Milk","Sut","Молоко","en","Milk"),
    ("Milk"," ","Молоко","uz","Milk"),
    ("Milk","Sut",null,"ru","Milk"),
    ("Milk"," Sut ","Молоко","uz","Sut"),
    (null,null,null,"uz","")
};
foreach (var test in cases) {
    var actual=CategoryDisplayName.Resolve(test.Fallback,test.Uz,test.Ru,test.Language);
    if(actual!=test.Expected) throw new Exception($"{test.Language}: expected {test.Expected}, got {actual}");
}
Console.WriteLine($"{cases.Length} category localization cases passed.");
