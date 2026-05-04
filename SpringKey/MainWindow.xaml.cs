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

        private void RenameTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (RenameTextBox.IsVisible)
            {
                RenameTextBox.Focus();
                RenameTextBox.CaretIndex = RenameTextBox.Text.Length;
            }
        }

        private void RenameGroupTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb && tb.IsVisible)
            {
                tb.Focus();
                tb.CaretIndex = tb.Text.Length;
            }
        }

        private void RenameGroupTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is not MainViewModel vm) return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (vm.IsRenamingGroup)
                    vm.ConfirmRenameGroupCommand.Execute(null);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void AddGroupTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (AddGroupTextBox.IsVisible)
            {
                AddGroupTextBox.Focus();
            }
        }

        private void GroupMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void GroupRenameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.MenuItem menuItem) return;
            if (FindContextMenuOwner(menuItem) is not System.Windows.Controls.Button btn) return;
            if (btn.DataContext is not string groupName) return;
            if (DataContext is MainViewModel vm)
                vm.RenameGroupCommand.Execute(groupName);
        }

        private void GroupDeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.MenuItem menuItem) return;
            if (FindContextMenuOwner(menuItem) is not System.Windows.Controls.Button btn) return;
            if (btn.DataContext is not string groupName) return;
            if (DataContext is MainViewModel vm)
                vm.DeleteGroupCommand.Execute(groupName);
        }

        private static System.Windows.Controls.Button? FindContextMenuOwner(System.Windows.Controls.MenuItem menuItem)
        {
            var parent = System.Windows.Media.VisualTreeHelper.GetParent(menuItem);
            while (parent != null)
            {
                if (parent is System.Windows.Controls.ContextMenu cm && cm.PlacementTarget is System.Windows.Controls.Button btn)
                    return btn;
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
}