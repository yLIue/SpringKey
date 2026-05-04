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
        
        private DateTime _lastCopyAccountTime = DateTime.MinValue;

        private LinkFile? userLink;

        private IndexFile? userIndex;

        private string appPath = AppDomain.CurrentDomain.BaseDirectory;
        
        private readonly IDialogService _dialogService;
        
        private readonly IPromptService _promptService;
        
        private KeyItemViewModel? _lastKey;

        #endregion

        #region MVVMDefinition

        public ICommand SignInCommand { get; }
        public ICommand SignOutCommand { get; }
        public ICommand AddFileCommand { get; }
        public ICommand ItemClickCommand { get; }
        public ICommand CopyClickCommand { get; }
        public ICommand RenameUserCommand { get; }
        public ICommand ConfirmEditUserNameCommand { get; }
        public ICommand CancelEditUserNameCommand { get; }
        public ICommand AddGroupCommand { get; }
        public ICommand ConfirmAddGroupCommand { get; }
        public ICommand CancelAddGroupCommand { get; }
        public ICommand RenameGroupCommand { get; }
        public ICommand ConfirmRenameGroupCommand { get; }
        public ICommand CancelRenameGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        
        private KeyItemViewModel? _selectedKey;

        public KeyItemViewModel? SelectedKey
        {
            get => _selectedKey;
            set
            {
                SetProperty(ref _selectedKey, value);
                if (_lastKey != null && !ReferenceEquals(_lastKey, _selectedKey))
                {
                    _lastKey.ShowVisibility = Visibility.Hidden;
                    _lastKey.CopyText = "copy";
                    ItemClick(_selectedKey);
                }
            }
        }

        private List<KeyItemViewModel>? _keys;

        public List<KeyItemViewModel>? Keys
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

        private bool _isEditingUserName;

        public bool IsEditingUserName
        {
            get => _isEditingUserName;
            set => SetProperty(ref _isEditingUserName, value);
        }

        private string _editingUserName = "";

        public string EditingUserName
        {
            get => _editingUserName;
            set => SetProperty(ref _editingUserName, value);
        }

        private bool _isAddingGroup;

        public bool IsAddingGroup
        {
            get => _isAddingGroup;
            set => SetProperty(ref _isAddingGroup, value);
        }

        private string _newGroupName = "";

        public string NewGroupName
        {
            get => _newGroupName;
            set => SetProperty(ref _newGroupName, value);
        }

        private bool _isRenamingGroup;

        public bool IsRenamingGroup
        {
            get => _isRenamingGroup;
            set => SetProperty(ref _isRenamingGroup, value);
        }

        private string _editingGroupOldName = "";

        public string EditingGroupOldName
        {
            get => _editingGroupOldName;
            set => SetProperty(ref _editingGroupOldName, value);
        }

        private string _editingGroupNewName = "";

        public string EditingGroupNewName
        {
            get => _editingGroupNewName;
            set => SetProperty(ref _editingGroupNewName, value);
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
            var testKeyInfoVm = new KeyItemViewModel(testKeyInfo);
            Keys = new List<KeyItemViewModel>();
            Keys.Add(testKeyInfoVm);
        }

        public MainViewModel(IDialogService dialogService, IPromptService promptService)
        {
            SignInCommand = new RelayCommand(SignIn);
            SignOutCommand = new RelayCommand(SignOut);
            AddFileCommand = new RelayCommand(AddFile);
            CopyClickCommand = new RelayCommand<KeyItemViewModel>(CopyClick);
            RenameUserCommand = new RelayCommand(StartEditUserName);
            ConfirmEditUserNameCommand = new RelayCommand(ConfirmEditUserName);
            CancelEditUserNameCommand = new RelayCommand(CancelEditUserName);
            AddGroupCommand = new RelayCommand(StartAddGroup);
            ConfirmAddGroupCommand = new RelayCommand(ConfirmAddGroup);
            CancelAddGroupCommand = new RelayCommand(CancelAddGroup);
            RenameGroupCommand = new RelayCommand<string>(StartRenameGroup);
            ConfirmRenameGroupCommand = new RelayCommand<string>(ConfirmRenameGroup);
            CancelRenameGroupCommand = new RelayCommand(CancelRenameGroup);
            DeleteGroupCommand = new RelayCommand<string>(DeleteGroup);
            appPath = Path.Combine(appPath, ".test");
            _dialogService = dialogService;
            _promptService = promptService;
            ItemClickCommand = new RelayCommand<KeyItemViewModel>(ItemClick);
            _promptService.PromptRequested += message => _ = PromptChange(message);
            Update();
        }

        private void CopyClick(KeyItemViewModel? key)
        {
            if (key == null) return;
            
            if (key.CopyText == "true")
            {
                _ = PromptChange("多次点击,冷却中");
                return;
            }
            key.CopyText = "true";
            Clipboard.SetText(key.Password);
            _ = PromptChange($"密码已复制: {key.Password}");
            _ = ResetCopyTextAsync(key);
        }

        private async Task ResetCopyTextAsync(KeyItemViewModel key)
        {
            await Task.Delay(2000);
            key.CopyText = "copy";
        }

        private void ItemClick(KeyItemViewModel? key)
        {
            if (key == null) return;
            if (_lastKey != null && !ReferenceEquals(_lastKey, key))
            {
                _lastKey.ShowVisibility = Visibility.Hidden;
                _lastKey.CopyText = "copy";
            }
            key.ShowVisibility = Visibility.Visible;
            if (!ReferenceEquals(_lastKey, key))
            {
                _lastKey = key;
                _ = PromptChange("第一次选择");
                return;
            }

            if ((DateTime.Now - _lastCopyAccountTime).TotalMilliseconds < 300)
            {
                _ = PromptChange("多次点击,冷却中");
                return;
            }
                
            _lastCopyAccountTime = DateTime.Now;
            Clipboard.SetText(key.Account);
            _ = PromptChange($"账号已复制,当前账号{key.Account}");
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

        private void StartEditUserName()
        {
            if (userLink == null) return;
            EditingUserName = UserName;
            IsEditingUserName = true;
        }

        private void ConfirmEditUserName()
        {
            if (userLink == null) return;
            if (string.IsNullOrEmpty(EditingUserName) || EditingUserName == UserName)
            {
                CancelEditUserName();
                return;
            }
            userLink.Rename(EditingUserName);
            UserName = userLink.UserName;
            UserInitial = UserName[0].ToString();
            userIndex = new IndexFile(userLink.UserPath, UserKey);
            GroupIndex = userIndex.GroupIndex;
            Keys = null;
            SelectedGroup = null;
            IsEditingUserName = false;
            _ = PromptChange($"用户名已更改为: {UserName}");
        }

        private void CancelEditUserName()
        {
            IsEditingUserName = false;
        }

        private void StartAddGroup()
        {
            if (userIndex == null) return;
            NewGroupName = "";
            IsAddingGroup = true;
        }

        private void ConfirmAddGroup()
        {
            if (userIndex == null) return;
            if (string.IsNullOrWhiteSpace(NewGroupName))
            {
                CancelAddGroup();
                return;
            }
            userIndex.AddNewGroup(NewGroupName.Trim());
            GroupIndex = userIndex.GroupIndex;
            IsAddingGroup = false;
            _ = PromptChange($"分组已添加: {NewGroupName.Trim()}");
        }

        private void CancelAddGroup()
        {
            IsAddingGroup = false;
            NewGroupName = "";
        }

        private void StartRenameGroup(string? oldName)
        {
            if (userIndex == null || string.IsNullOrEmpty(oldName)) return;
            if (oldName == "全部" || oldName == "未分类") return;
            EditingGroupOldName = oldName;
            EditingGroupNewName = oldName;
            IsRenamingGroup = true;
        }

        private void ConfirmRenameGroup(string? unused)
        {
            if (userIndex == null) return;
            var oldName = EditingGroupOldName;
            var newName = EditingGroupNewName?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(newName) || oldName == newName || string.IsNullOrEmpty(oldName))
            {
                CancelRenameGroup();
                return;
            }
            if (!userIndex.RenameGroup(oldName, newName))
            {
                _ = PromptChange($"重命名失败: 分组名已存在或无效");
                CancelRenameGroup();
                return;
            }
            GroupIndex = userIndex.GroupIndex;
            if (SelectedGroup == oldName)
                SelectedGroup = newName;
            IsRenamingGroup = false;
            _ = PromptChange($"分组已重命名: {oldName} → {newName}");
        }

        private void CancelRenameGroup()
        {
            IsRenamingGroup = false;
            EditingGroupOldName = "";
            EditingGroupNewName = "";
        }

        private void DeleteGroup(string? groupName)
        {
            if (userIndex == null || string.IsNullOrEmpty(groupName)) return;
            if (groupName == "全部" || groupName == "未分类") return;
            if (!SpringKey.View.ConfirmDialog.Show(
                $"确定要删除分组 \"{groupName}\" 吗？\n其中的密码项将移至「未分类」。",
                "删除分组"))
                return;
            userIndex.DeleteGroup(groupName);
            GroupIndex = userIndex.GroupIndex;
            if (SelectedGroup == groupName)
                SelectedGroup = null;
            Keys = null;
            _ = PromptChange($"分组已删除: {groupName}");
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
            Keys = null;
            SelectedGroup = null;
            _lastKey = null;
            LoginVisibilityChange();
            _ = PromptChange($"SignOut");
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
