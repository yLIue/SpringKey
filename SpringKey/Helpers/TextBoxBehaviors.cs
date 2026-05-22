using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpringKey.Helpers
{
    public static class TextBoxBehaviors
    {
        public static readonly DependencyProperty FocusAndSelectOnVisibleProperty =
            DependencyProperty.RegisterAttached(
                "FocusAndSelectOnVisible",
                typeof(bool),
                typeof(TextBoxBehaviors),
                new PropertyMetadata(false, OnFocusAndSelectOnVisibleChanged));

        public static bool GetFocusAndSelectOnVisible(DependencyObject obj) =>
            (bool)obj.GetValue(FocusAndSelectOnVisibleProperty);

        public static void SetFocusAndSelectOnVisible(DependencyObject obj, bool value) =>
            obj.SetValue(FocusAndSelectOnVisibleProperty, value);

        private static void OnFocusAndSelectOnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox tb) return;
            tb.IsVisibleChanged -= OnTextBoxVisibleChanged;
            if ((bool)e.NewValue)
                tb.IsVisibleChanged += OnTextBoxVisibleChanged;
        }

        private static void OnTextBoxVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not TextBox tb || !tb.IsVisible) return;
            tb.Focus();
            tb.CaretIndex = tb.Text.Length;
        }

        public static readonly DependencyProperty LostFocusCommandProperty =
            DependencyProperty.RegisterAttached(
                "LostFocusCommand",
                typeof(ICommand),
                typeof(TextBoxBehaviors),
                new PropertyMetadata(null, OnLostFocusCommandChanged));

        public static ICommand GetLostFocusCommand(DependencyObject obj) =>
            (ICommand)obj.GetValue(LostFocusCommandProperty);

        public static void SetLostFocusCommand(DependencyObject obj, ICommand value) =>
            obj.SetValue(LostFocusCommandProperty, value);

        private static void OnLostFocusCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox tb) return;
            tb.LostFocus -= OnTextBoxLostFocus;
            if (e.NewValue is ICommand)
                tb.LostFocus += OnTextBoxLostFocus;
        }

        private static void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox tb) return;
            var command = GetLostFocusCommand(tb);
            if (command == null) return;
            tb.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (command.CanExecute(null))
                    command.Execute(null);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
