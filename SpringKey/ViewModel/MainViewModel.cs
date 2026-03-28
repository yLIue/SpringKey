using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SpringKey.Files;
using SpringKey.MVVM;
using SpringKey.Services;

namespace SpringKey.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        #region Definition
        
        private const string DefaultPrompt = "这里是提示语句";

        private CancellationTokenSource? _cts;

        private LinkFile? userLink;

        private IndexFile? userIndex;

        private string appPath = AppDomain.CurrentDomain.BaseDirectory;
        
        private readonly IDialogService _dialogService;

        #endregion

        #region MVVMDefinition
        public ICommand SignInCommand { get; }
        public ICommand SignOutCommand { get; }
        public ICommand AddFileCommand { get; }

        private string _selectedGroup;

        public string SelectedGroup
        {
            get => _selectedGroup;
            set 
            {
                SetProperty(ref _selectedGroup, value);
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
            set => SetProperty(ref _prompt, value);
        }
        #endregion

        public MainViewModel() : this(new DialogService())
        {
        }

        public MainViewModel(IDialogService dialogService)
        {
            SignInCommand = new RelayCommand(SignIn);
            SignOutCommand = new RelayCommand(SignOut);
            AddFileCommand = new RelayCommand(AddFile);
            appPath = Path.Combine(appPath, ".test");
            _dialogService = dialogService;
        }

        private void AddFile()
        {
            _dialogService.ShowAddFileView();
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
