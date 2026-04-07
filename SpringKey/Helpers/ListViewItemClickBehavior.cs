using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpringKey.Helpers
{
    // 同理 这段也是AI写的TvT
    // 其实我看不懂这是怎么实现的 黑盒了 但还能接受
    public static class ListViewItemClickBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(ListViewItemClickBehavior),
                new PropertyMetadata(null, OnCommandChanged));

        public static void SetCommand(DependencyObject element, ICommand value)
            => element.SetValue(CommandProperty, value);

        public static ICommand GetCommand(DependencyObject element)
            => (ICommand)element.GetValue(CommandProperty);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListViewItem item)
            {
                item.PreviewMouseLeftButtonDown -= Item_PreviewMouseLeftButtonDown;
                if (e.NewValue != null)
                    item.PreviewMouseLeftButtonDown += Item_PreviewMouseLeftButtonDown;
            }
        }

        private static void Item_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not ListViewItem item)
                return;

            if (IsInteractiveElement(e.OriginalSource as DependencyObject))
                return;

            var command = GetCommand(item);
            var parameter = item.DataContext;

            if (command?.CanExecute(parameter) == true)
                command.Execute(parameter);
        }

        private static bool IsInteractiveElement(DependencyObject? current)
        {
            while (current != null)
            {
                if (current is System.Windows.Controls.Primitives.ButtonBase ||
                    current is TextBox ||
                    current is ComboBox)
                {
                    return true;
                }

                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }

            return false;
        }
    }
}