using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SpringKey.MVVM;

namespace SpringKey.ViewModel
{
    class MainViewModel : ViewModelBase
    {
        private const string DefaultPrompt = "这里是提示语句";
        private string prompt = DefaultPrompt;
        public string Prompt
        {
            get => prompt;
            set => SetProperty(ref prompt, value);
        }
    }
}
