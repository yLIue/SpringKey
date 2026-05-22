using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Data;

namespace SpringKey.Helpers
{
    // 笑死，这PasswordBox写MVVM存坐牢，这段代码对于我这个初学者来讲已经是看不懂了
    // 选这个方案是因为PasswordBox的实现方法我看不懂的更多(不知道怎么实现KeyBindingEnter)，所以只好使用这个了
    // TvT
    public static class MaskedTextBoxHelper
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(MaskedTextBoxHelper),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject obj, bool value)
            => obj.SetValue(IsEnabledProperty, value);

        public static bool GetIsEnabled(DependencyObject obj)
            => (bool)obj.GetValue(IsEnabledProperty);

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached(
                "Password",
                typeof(string),
                typeof(MaskedTextBoxHelper),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnPasswordChanged));

        public static void SetPassword(DependencyObject obj, string value)
            => obj.SetValue(PasswordProperty, value);

        public static string GetPassword(DependencyObject obj)
            => (string)obj.GetValue(PasswordProperty);

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached(
                "IsUpdating",
                typeof(bool),
                typeof(MaskedTextBoxHelper),
                new PropertyMetadata(false));

        private static bool GetIsUpdating(DependencyObject obj)
            => (bool)obj.GetValue(IsUpdatingProperty);

        private static void SetIsUpdating(DependencyObject obj, bool value)
            => obj.SetValue(IsUpdatingProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox tb) return;

            if ((bool)e.NewValue)
            {
                tb.PreviewTextInput += Tb_PreviewTextInput;
                tb.PreviewKeyDown += Tb_PreviewKeyDown;
                DataObject.AddPastingHandler(tb, Tb_OnPaste);
            }
            else
            {
                tb.PreviewTextInput -= Tb_PreviewTextInput;
                tb.PreviewKeyDown -= Tb_PreviewKeyDown;
                DataObject.RemovePastingHandler(tb, Tb_OnPaste);
            }
        }

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox tb) return;
            if (GetIsUpdating(tb)) return;

            SetIsUpdating(tb, true);
            tb.Text = Mask((string?)e.NewValue);
            tb.CaretIndex = tb.Text.Length;
            SetIsUpdating(tb, false);
        }

        private static void Tb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is not TextBox tb) return;

            SetIsUpdating(tb, true);

            string password = GetPassword(tb) ?? string.Empty;
            int start = tb.SelectionStart;
            int length = tb.SelectionLength;

            password = ReplaceRange(password, start, length, e.Text);

            SetPassword(tb, password);
            tb.Text = Mask(password);
            tb.SelectionStart = start + e.Text.Length;
            tb.SelectionLength = 0;

            SetIsUpdating(tb, false);
            e.Handled = true;
        }

        private static void Tb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not TextBox tb) return;

            if (e.Key != Key.Back && e.Key != Key.Delete)
                return;

            SetIsUpdating(tb, true);

            string password = GetPassword(tb) ?? string.Empty;
            int start = tb.SelectionStart;
            int length = tb.SelectionLength;

            if (length == 0)
            {
                if (e.Key == Key.Back && start > 0)
                {
                    start -= 1;
                    length = 1;
                }
                else if (e.Key == Key.Delete && start < password.Length)
                {
                    length = 1;
                }
            }

            password = ReplaceRange(password, start, length, "");

            SetPassword(tb, password);
            tb.Text = Mask(password);
            tb.SelectionStart = start;
            tb.SelectionLength = 0;

            SetIsUpdating(tb, false);
            e.Handled = true;
        }

        private static void Tb_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is not TextBox tb) return;
            if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText)) return;

            string pasteText = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string ?? string.Empty;

            SetIsUpdating(tb, true);

            string password = GetPassword(tb) ?? string.Empty;
            int start = tb.SelectionStart;
            int length = tb.SelectionLength;

            password = ReplaceRange(password, start, length, pasteText);

            SetPassword(tb, password);
            tb.Text = Mask(password);
            tb.SelectionStart = start + pasteText.Length;
            tb.SelectionLength = 0;

            SetIsUpdating(tb, false);
            e.CancelCommand();
        }

        private static string Mask(string? text)
            => string.IsNullOrEmpty(text) ? string.Empty : new string('*', text.Length);

        private static string ReplaceRange(string source, int start, int length, string insert)
        {
            source ??= string.Empty;
            insert ??= string.Empty;

            if (start < 0) start = 0;
            if (start > source.Length) start = source.Length;
            if (length < 0) length = 0;
            if (start + length > source.Length) length = source.Length - start;

            return source.Remove(start, length).Insert(start, insert);
        }
    }
}