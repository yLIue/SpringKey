using System.IO;
using System.Windows;
using System.Windows.Input;
using SpringKey.Files;
using SpringKey.MVVM;
using SpringKey.Services;
using SpringKey.Struct;

namespace SpringKey.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        #region Definition
        
        private const string DefaultPrompt = "这里是提示语句";

        private CancellationTokenSource? _cts;
        
        private DateTime _lastCopyTime = DateTime.MinValue;

        private LinkFile? userLink;

        private IndexFile? userIndex;

        private string appPath = AppDomain.CurrentDomain.BaseDirectory;
        
        private readonly IDialogService _dialogService;
        
        private readonly IPromptService _promptService;

        #endregion

        #region MVVMDefinition
        public ICommand SignInCommand { get; }
        public ICommand SignOutCommand { get; }
        public ICommand AddFileCommand { get; }
        public ICommand ItemClickCommand { get; }
        
        private KeyInfo? _selectedKey;

        public KeyInfo? SelectedKey
        {
            get => _selectedKey;
            set => SetProperty(ref _selectedKey, value);
        }

        private List<KeyInfo>? _keys;

        public List<KeyInfo>? Keys
        {
            get => _keys;
            set => SetProperty(ref _keys, value);
        }

        private string? _selectedGroup;

        public string? SelectedGroup
        {
            get => _selectedGroup;
            set 
            {
                SetProperty(ref _selectedGroup, value);
                Update();
                _ = PromptChange($"Surveillance: {_selectedGroup}");
            }
        }


        private IReadOnlyList<String> ?_groupIndex;

        public IReadOnlyList<String> ?GroupIndex
        {
            get => _groupIndex;
            set => SetProperty(ref _groupIndex, value);
        }


        private string ?_userInitial;

        public string ?UserInitial
        {
            get => _userInitial;
            set => SetProperty(ref _userInitial, value);
        }


        private string ?_userName;

        public string ?UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }


        private Visibility _userVisibility = Visibility.Hidden;

        public Visibility UserVisibility
        {
            get => _userVisibility;
            set => SetProperty(ref _userVisibility, value);
        }


        private Visibility _loginVisibility = Visibility.Visible;

        public Visibility LoginVisibility
        {
            get => _loginVisibility;
            set => SetProperty(ref _loginVisibility, value);
        }


        private string _userKey = "";

        public string UserKey
        {
            get => _userKey;
            set => SetProperty(ref _userKey, value);
        }

        private string _prompt = DefaultPrompt;
        public string Prompt
        {
            get => _prompt;
            set
            {
                System.Diagnostics.Debug.WriteLine($"Prompt setter 被调用，新值：{value}");
                SetProperty(ref _prompt, value);
            }
        }

        #endregion

        // 这是给设计器看的
        public MainViewModel() : this(new DialogService(new PromptService()),new PromptService())
        {
            var testKey = new KeyFile("测试key标题","213875818","isPassword");
            var testKeyInfo = new KeyInfo(testKey,"hash","全部");
            Keys = new List<KeyInfo>();
            Keys.Add(testKeyInfo);
        }

        public MainViewModel(IDialogService dialogService, IPromptService promptService)
        {
            SignInCommand = new RelayCommand(SignIn);
            SignOutCommand = new RelayCommand(SignOut);
            AddFileCommand = new RelayCommand(AddFile);
            appPath = Path.Combine(appPath, ".test");
            _dialogService = dialogService;
            _promptService = promptService;
            ItemClickCommand = new RelayCommand<KeyInfo>(ItemClick);
            _promptService.PromptRequested += message => _ = PromptChange(message);
            Update();
        }

        private void ItemClick(KeyInfo? key)
        {
            if (key == null) return;
            if (!ReferenceEquals(SelectedKey, key))
            {
                SelectedKey = key;
                _ = PromptChange("第一次选择");
                return;
            }

            if ((DateTime.Now - _lastCopyTime).TotalMilliseconds < 300)
            {
                _ = PromptChange("多次点击,冷却中");
                return;
            }
                
            _lastCopyTime = DateTime.Now;
            Clipboard.SetText(key.Password);
            _ = PromptChange($"账号已复制,当前密码{key.Account}");
        }
        
        private void Update()
        {
            if (userLink == null)
                return;
            if (!string.IsNullOrEmpty(_selectedGroup))
            {
                Keys = userIndex!.GetGroupInfo(_selectedGroup!);    
            }
        }

        private void AddFile()
        {
            var parameter = new AddKeyParameter(userIndex!, SelectedGroup!);
            _dialogService.ShowAddFileView(parameter);
            Update();
        }

        private void LoadData()
        {
            userLink = new LinkFile(appPath, UserKey);
            userIndex = new IndexFile(userLink.UserPath,UserKey);
            UserName = userLink.UserName;
            UserInitial = UserName[0].ToString();
            GroupIndex = userIndex.GroupIndex;
        }

        private void UninstallData()
        {
            userLink = null;
            UserName = null;
            UserInitial = null;
            userIndex = null;
            UserKey = "";
            GroupIndex = null;
        }

        private void SignIn()
        {
            if (UserKey == "")
            {
                _ = PromptChange($"EnterInputKey: key为空");
                return;
            }

            LoadData();

            LoginVisibilityChange();
            _ = PromptChange($"EnterInputKey: {UserKey}");
        }

        private void SignOut()
        {
            UninstallData();
            LoginVisibilityChange();
            _ = PromptChange($"SignOut"); 
            Update();
        }

        #region utils
        private async Task PromptChange(string prompt)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            Prompt = prompt;

            try
            {
                await Task.Delay(5000, _cts.Token);
                Prompt = DefaultPrompt;
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void LoginVisibilityChange()
        {
            if (LoginVisibility == Visibility.Visible)
            {
                LoginVisibility = Visibility.Hidden;
                UserVisibility = Visibility.Visible;

            }
            else
            {
                LoginVisibility = Visibility.Visible;
                UserVisibility = Visibility.Hidden;
            }
        }
        #endregion
    }
}
