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

namespace SpringKey.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        #region Definition
        
        private const string DefaultPrompt = "这里是提示语句";

        private CancellationTokenSource? _cts;

        private LinkFile? userLink;

        private string appPath = AppDomain.CurrentDomain.BaseDirectory;

        #endregion

        #region MVVMDefinition
        public ICommand SignInCommand { get; }

        public ICommand SignOutCommand { get; }

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

        public MainViewModel()
        {
            SignInCommand = new RelayCommand(SignIn);
            SignOutCommand = new RelayCommand(SignOut);
            appPath = Path.Combine(appPath, ".test");
        }

        private void SignOut()
        {
            userLink = null;
            LoginVisibilityChange();
            _ = PromptChange($"SignOut");
            UserKey = "";
        }

        private void SignIn()
        {
            if (UserKey == "")
            {
                _ = PromptChange($"EnterInputKey: key为空");
                return;
            }

            //userLink = new LinkFile(appPath, UserKey);

            LoginVisibilityChange();
            _ = PromptChange($"EnterInputKey: {UserKey}");
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
