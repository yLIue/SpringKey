using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SpringKey.View
{
    public partial class GroupPickerDialog : Window
    {
        public string? SelectedGroup { get; private set; }

        public GroupPickerDialog()
        {
            InitializeComponent();
        }

        public static string? Show(IEnumerable<string> groups)
        {
            var dialog = new GroupPickerDialog
            {
                Owner = Application.Current.MainWindow
            };
            var groupList = groups.ToList();
            dialog.GroupListBox.ItemsSource = groupList;
            if (groupList.Count > 0)
                dialog.GroupListBox.SelectedIndex = 0;
            return dialog.ShowDialog() == true ? dialog.SelectedGroup : null;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedGroup = GroupListBox.SelectedItem as string;
            if (SelectedGroup == null) return;
            DialogResult = true;
            Close();
        }
    }
}
