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

        private void KeyMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void KeyContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.ContextMenu menu) return;
            if (menu.PlacementTarget is not System.Windows.Controls.Button btn) return;
            if (btn.DataContext is not KeyItemViewModel keyVm) return;
            if (DataContext is not MainViewModel vm) return;

            BuildMainMenu(menu, vm, keyVm);
        }

        private void BuildMainMenu(System.Windows.Controls.ContextMenu menu, MainViewModel vm, KeyItemViewModel keyVm)
        {
            menu.Items.Clear();

            var editItem = new System.Windows.Controls.MenuItem { Header = "修改" };
            editItem.Click += (_, _) => vm.EditKeyCommand.Execute(keyVm);
            menu.Items.Add(editItem);

            if (keyVm.Group is not "全部" and not "未分类")
            {
                var removeItem = new System.Windows.Controls.MenuItem { Header = "移除分组" };
                removeItem.Click += (_, _) => vm.RemoveFromGroupCommand.Execute(keyVm);
                menu.Items.Add(removeItem);
                menu.Items.Add(new System.Windows.Controls.Separator());
            }

            var deleteItem = new System.Windows.Controls.MenuItem { Header = "删除密码" };
            deleteItem.Click += (_, _) => vm.DeleteKeyCommand.Execute(keyVm);
            menu.Items.Add(deleteItem);
        }

    }
}