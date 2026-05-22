using System.Collections.ObjectModel;
using System.Windows.Input;
using SpringKey.Files;
using SpringKey.MVVM;
using SpringKey.Services;
using SpringKey.Struct;

namespace SpringKey.ViewModel;

class AddFileViewModel : ViewModelBase
{
    #region Services

    private readonly IDialogService _dialogService;
    private readonly IPromptService _promptService;
    readonly AddKeyParameter _addKeyParameter;
    private readonly string _originalPassword = "";

    #endregion

    #region Commands

    public ICommand BackCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand AddBindingCommand { get; }
    public ICommand RemoveBindingCommand { get; }
    public ICommand SelectPrevPasswordCommand { get; }
    public ICommand DeletePrevPasswordCommand { get; }

    #endregion

    #region Properties

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

    private string _description = "";
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public ObservableCollection<BindingItem> BindingList { get; } = new();
    public ObservableCollection<string> PasswordPrevList { get; } = new();
    public List<string> BindingTypes { get; } = new() { "phone", "email" };

    private bool _isPasswordHistoryOpen;
    public bool IsPasswordHistoryOpen
    {
        get => _isPasswordHistoryOpen;
        set => SetProperty(ref _isPasswordHistoryOpen, value);
    }

    #endregion

    #region Constructors

    public AddFileViewModel() : this(new DialogService(new PromptService()), new PromptService(), new AddKeyParameter())
    {
    }

    public AddFileViewModel(IDialogService dialogService, IPromptService promptService, AddKeyParameter addKeyParameter)
    {
        BackCommand = new RelayCommand(Back);
        SaveCommand = new RelayCommand(Save, IsSave);
        AddBindingCommand = new RelayCommand(AddBinding);
        RemoveBindingCommand = new RelayCommand<BindingItem>(RemoveBinding);
        SelectPrevPasswordCommand = new RelayCommand<string>(SelectPrevPassword);
        DeletePrevPasswordCommand = new RelayCommand<string>(DeletePrevPassword);

        _dialogService = dialogService;
        _promptService = promptService;
        _addKeyParameter = addKeyParameter;

        if (addKeyParameter.ExistingKey is { } ek)
        {
            _title = ek.Title;
            _account = ek.Account;
            _password = ek.Password;
            _originalPassword = ek.Password;
            _place = ek.Place;
            _description = ek.Description;
            foreach (var p in ek.PasswordPrev)
                PasswordPrevList.Add(p);
            foreach (var kv in ek.Binding)
                BindingList.Add(new BindingItem { Type = kv.Key, Value = kv.Value });
        }
    }

    #endregion

    #region Command Handlers

    private void Back()
    {
        _promptService.Show("取消");
        _dialogService.CloseAddFileView();
    }

    private void Save()
    {
        KeyFile key = new KeyFile(_title, _account, _password)
        {
            Place = _place,
            Description = _description
        };

        foreach (var bi in BindingList)
            if (!string.IsNullOrWhiteSpace(bi.Type) && !string.IsNullOrWhiteSpace(bi.Value))
                key.AddBinding(bi.Type, bi.Value);

        if (_addKeyParameter.ExistingKey is { } ek)
        {
            key.AddPasswordPrevEntries(PasswordPrevList);
            if (_originalPassword != _password)
                key.RecordPassword(_originalPassword);
            _addKeyParameter.Index.UpdataKey(ek, key);
        }
        else
        {
            _addKeyParameter.Index.AddKey(key, _addKeyParameter.Group);
        }

        _promptService.Show("保存成功");
        _dialogService.CloseAddFileView();
    }

    private bool IsSave()
    {
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Account) || string.IsNullOrWhiteSpace(Password))
            return false;
        return true;
    }

    private void AddBinding()
    {
        BindingList.Add(new BindingItem());
    }

    private void RemoveBinding(BindingItem item)
    {
        BindingList.Remove(item);
    }

    private void TogglePasswordHistory()
    {
        IsPasswordHistoryOpen = !IsPasswordHistoryOpen;
    }

    private void SelectPrevPassword(string password)
    {
        Password = password;
        IsPasswordHistoryOpen = false;
    }

    private void DeletePrevPassword(string password)
    {
        PasswordPrevList.Remove(password);
        if (PasswordPrevList.Count == 0)
            IsPasswordHistoryOpen = false;
    }

    #endregion
}
