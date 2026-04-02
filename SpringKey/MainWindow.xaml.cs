using System.IO;
using System.Text;
using System.Windows;
using SpringKey.ViewModel;
using SpringKey.Services;

using SpringKey.Test;

namespace SpringKey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var promptService = new PromptService();
            var dialogService = new DialogService(promptService);
            var vm = new MainViewModel(dialogService, promptService);
            DataContext = vm;
        }
    }
}