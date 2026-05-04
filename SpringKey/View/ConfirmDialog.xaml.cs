using System.Windows;

namespace SpringKey.View
{
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog()
        {
            InitializeComponent();
        }

        public static bool Show(string message, string title, string confirmText = "删除")
        {
            var dialog = new ConfirmDialog
            {
                Owner = Application.Current.MainWindow,
                Title = title
            };
            dialog.MessageText.Text = message;
            dialog.ConfirmButton.Content = confirmText;
            return dialog.ShowDialog() == true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
