using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Ninimum.Models;

namespace Ninimum.Views.Children;

public partial class ChildInfoPage : BasePage, INotifyPropertyChanged
{
    private readonly ObservableCollection<ChildrenInfo> _childrenList;
    private readonly ChildrenInfo? _editingChild;
    private readonly bool _isEditMode;

    public ICommand SaveCommand { get; }
    public ICommand SelectGenderCommand { get; }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public ChildInfoPage(ObservableCollection<ChildrenInfo> childrenList, ChildrenInfo? child = null)
    {
        InitializeComponent();

        _childrenList = childrenList;
        _editingChild = child;
        _isEditMode = child != null;

        SaveCommand = new Command(OnSave);
        SelectGenderCommand = new Command<string>(OnGenderSelected);

        if (_isEditMode && child != null)
        {
            PageTitle = "Farzand ma’lumotini tahrirlash";
            ButtonText = "Tahrirlash";

            ChildFirstName = child.FirstName;
            ChildLastName = child.LastName;
            ChildBirthDate = child.BirthDate;
            Gender = child.Gender;

            IsBoySelected = Gender == "Erkak";
            IsGirlSelected = Gender == "Ayol";
        }
        else
        {
            PageTitle = "Farzand qo‘shish";
            ButtonText = "Qo‘shish";

            Gender = "Erkak";
            IsBoySelected = true;
            IsGirlSelected = false;
        }

        BindingContext = this;
    }

    private string _pageTitle = string.Empty;
    public string PageTitle
    {
        get => _pageTitle;
        set
        {
            if (_pageTitle == value) return;
            _pageTitle = value;
            OnPropertyChanged();
        }
    }

    private string _buttonText = string.Empty;
    public string ButtonText
    {
        get => _buttonText;
        set
        {
            if (_buttonText == value) return;
            _buttonText = value;
            OnPropertyChanged();
        }
    }

    private string _childFirstName = string.Empty;
    public string ChildFirstName
    {
        get => _childFirstName;
        set
        {
            if (_childFirstName == value) return;
            _childFirstName = value;
            OnPropertyChanged();
        }
    }

    private string _childLastName = string.Empty;
    public string ChildLastName
    {
        get => _childLastName;
        set
        {
            if (_childLastName == value) return;
            _childLastName = value;
            OnPropertyChanged();
        }
    }

    private string _childBirthDate = string.Empty;
    public string ChildBirthDate
    {
        get => _childBirthDate;
        set
        {
            if (_childBirthDate == value) return;
            _childBirthDate = value;
            OnPropertyChanged();
        }
    }

    private string _gender = "Erkak";
    public string Gender
    {
        get => _gender;
        set
        {
            if (_gender == value) return;
            _gender = value;
            OnPropertyChanged();
        }
    }

    private bool _isBoySelected;
    public bool IsBoySelected
    {
        get => _isBoySelected;
        set
        {
            if (_isBoySelected == value) return;
            _isBoySelected = value;
            OnPropertyChanged();
        }
    }

    private bool _isGirlSelected;
    public bool IsGirlSelected
    {
        get => _isGirlSelected;
        set
        {
            if (_isGirlSelected == value) return;
            _isGirlSelected = value;
            OnPropertyChanged();
        }
    }

    private void OnGenderSelected(string gender)
    {
        if (gender == "boy")
        {
            IsBoySelected = true;
            IsGirlSelected = false;
            Gender = "Erkak";
        }
        else
        {
            IsBoySelected = false;
            IsGirlSelected = true;
            Gender = "Ayol";
        }
    }

    private async void OnSave()
    {
        if (_isEditMode && _editingChild != null)
        {
            _editingChild.FirstName = ChildFirstName;
            _editingChild.LastName = ChildLastName;
            _editingChild.BirthDate = ChildBirthDate;
            _editingChild.Gender = Gender;
            _editingChild.Age = "5 yosh";
        }
        else
        {
            _childrenList.Add(new ChildrenInfo
            {
                FirstName = ChildFirstName,
                LastName = ChildLastName,
                BirthDate = ChildBirthDate,
                Gender = Gender,
                Age = "5 yosh"
            });
        }

        await Navigation.PopAsync();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}