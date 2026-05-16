using System.Windows.Input;
using SpringKey.Files;
using SpringKey.MVVM;
using SpringKey.Services;
using SpringKey.Struct;

namespace SpringKey.ViewModel;

class AddFileViewModel : ViewModelBase
{
    #region Definition
    
    private readonly IDialogService _dialogService;
    
    private readonly IPromptService _promptService;

    readonly AddKeyParameter _addKeyParameter;
    
    #endregion
    
    #region MVVMDefinition
    public ICommand BackCommand { get; }
    public ICommand SaveCommand { get; }

    private string _title = "未命名的标题";

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _account = "";

    public string Account
    {
        get => _account;
        set => SetProperty(ref _account, value);
    }

    private string _password = "";

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string _place = "";

    public string Place
    {
        get => _place;
        set => SetProperty(ref _place, value);
    }

    #endregion

    public AddFileViewModel() : this(new DialogService(new PromptService()),new PromptService(),new AddKeyParameter())
    {
        
    }
    
    public AddFileViewModel(IDialogService dialogService,IPromptService promptService,AddKeyParameter addKeyParameter)
    {
        BackCommand = new RelayCommand(Back);
        SaveCommand = new RelayCommand(Save,IsSave);
        _dialogService = dialogService;
        _promptService = promptService;
        _addKeyParameter = addKeyParameter;
        if (addKeyParameter.ExistingKey is { } ek)
        {
            _title = ek.Title;
            _account = ek.Account;
            _password = ek.Password;
            _place = ek.Place;
        }
    }
    
    private void Back()
    {
        _promptService.Show("取消");
        _dialogService.CloseAddFileView();
    }
    
    private void Save()
    {
        KeyFile key = new KeyFile(_title,_account,_password)
        {
            Place = _place
        };
        if (_addKeyParameter.ExistingKey is { } ek)
            _addKeyParameter.Index.UpdataKey(ek, key);
        else
            _addKeyParameter.Index.AddKey(key,_addKeyParameter.Group);
        _promptService.Show("保存成功");
        _dialogService.CloseAddFileView();
    }

    private bool IsSave()
    {
        if(string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Account) || string.IsNullOrEmpty(Password))
            return false;
        return true;
    }
}