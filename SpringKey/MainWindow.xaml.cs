using System.Windows;
using SpringKey.ViewModel;
using SpringKey.Services;

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

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is MainViewModel vm)
                vm.Cleanup();
            base.OnClosed(e);
        }


        private void GroupMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }


        private void KeyMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

    }
}