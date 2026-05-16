using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpringKey.Helpers
{
    public static class ComboBoxHelper
    {
        public static readonly DependencyProperty DisableSelectAllProperty =
            DependencyProperty.RegisterAttached(
                "DisableSelectAll",
                typeof(bool),
                typeof(ComboBoxHelper),
                new PropertyMetadata(false, OnDisableSelectAllChanged));

        private static readonly DependencyProperty IsHookedProperty =
            DependencyProperty.RegisterAttached(
                "IsHooked", typeof(bool), typeof(ComboBoxHelper));

        public static bool GetDisableSelectAll(DependencyObject obj) =>
            (bool)obj.GetValue(DisableSelectAllProperty);

        public static void SetDisableSelectAll(DependencyObject obj, bool value) =>
            obj.SetValue(DisableSelectAllProperty, value);

        private static void OnDisableSelectAllChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ComboBox cb) return;
            cb.Loaded -= OnComboBoxLoaded;
            if ((bool)e.NewValue)
            {
                if (cb.IsLoaded)
                    HookTextBox(cb);
                else
                    cb.Loaded += OnComboBoxLoaded;
            }
        }

        private static void OnComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not ComboBox cb) return;
            cb.Loaded -= OnComboBoxLoaded;
            HookTextBox(cb);
        }

        private static void HookTextBox(ComboBox cb)
        {
            if ((bool)cb.GetValue(IsHookedProperty)) return;
            cb.SetValue(IsHookedProperty, true);

            cb.Dispatcher.BeginInvoke(new Action(() =>
            {
                var tb = cb.Template?.FindName("PART_EditableTextBox", cb) as TextBox;
                if (tb == null) return;
                tb.GotKeyboardFocus += OnTextBoxGotKeyboardFocus;
                cb.Unloaded += (_, _) =>
                {
                    tb.GotKeyboardFocus -= OnTextBoxGotKeyboardFocus;
                    cb.ClearValue(IsHookedProperty);
                };
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private static void OnTextBoxGotKeyboardFocus(object? sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is not TextBox tb) return;
            tb.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (tb.SelectionLength == tb.Text.Length)
                    tb.Select(tb.Text.Length, 0);
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}
