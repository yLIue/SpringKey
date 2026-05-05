using System.Windows;
using SpringKey.MVVM;
using SpringKey.Struct;
using SpringKey.Files;

namespace SpringKey.ViewModel;

public class KeyItemViewModel(KeyInfo model) : ViewModelBase
{
    private readonly KeyInfo _model = model;
    public string Title => _model.Title;
    public string Account => _model.Account;
    public string Password => _model.Password;
    public string Hash => _model.Hash;
    public string Group => _model.Group;

    private string _copyText = "copy";
    public string CopyText
    {
        get => _copyText;
        set => SetProperty(ref _copyText, value);
    }
    
    private Visibility _showVisibility = Visibility.Hidden;
    public Visibility ShowVisibility
    {
        get => _showVisibility;
        set => SetProperty(ref _showVisibility, value);
    }
    
    public KeyItemViewModel(KeyFile key, string hash, string group)
        : this(new KeyInfo(key, hash, group))
    {
    }

    internal KeyInfo Model => _model;
}