using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dotc.Wpf
{
    public class SelectTextOnFocus : DependencyObject
    {
        public static readonly DependencyProperty ActiveProperty = DependencyProperty.RegisterAttached(
            "Active",
            typeof(bool),
            typeof(SelectTextOnFocus),
            new PropertyMetadata(false, ActivePropertyChanged));

        private static void ActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBox)) return;
            var textBox = (TextBox) d;
            if ((e.NewValue as bool?).GetValueOrDefault(false))
            {
                WeakEventManager<TextBox, KeyboardFocusChangedEventArgs>
                    .AddHandler(textBox, "GotKeyboardFocus", OnKeyboardFocusSelectText);
                WeakEventManager<TextBox, MouseButtonEventArgs>
                    .AddHandler(textBox, "PreviewMouseLeftButtonDown", OnMouseLeftButtonDown);
            }
            else
            {
                WeakEventManager<TextBox, KeyboardFocusChangedEventArgs>
                    .RemoveHandler(textBox, "GotKeyboardFocus", OnKeyboardFocusSelectText);
                WeakEventManager<TextBox, MouseButtonEventArgs>
                    .RemoveHandler(textBox, "PreviewMouseLeftButtonDown", OnMouseLeftButtonDown);
            }
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dependencyObject = GetParentFromVisualTree(e.OriginalSource);

            if (dependencyObject == null)
            {
                return;
            }

            var textBox = (TextBox)dependencyObject;
            if (textBox.IsKeyboardFocusWithin) return;
            textBox.Focus();
            e.Handled = true;
        }

        private static DependencyObject GetParentFromVisualTree(object source)
        {
            DependencyObject parent = source as UIElement;
            while (parent != null && !(parent is TextBox))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
        }

        private static void OnKeyboardFocusSelectText(object sender, KeyboardFocusChangedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            textBox?.SelectAll();
        }

        [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(TextBox))]
        public static bool GetActive(DependencyObject dp)
        {
            if (dp == null) throw new ArgumentNullException(nameof(dp));

            return (bool)dp.GetValue(ActiveProperty);
        }

        public static void SetActive(DependencyObject dp, bool value)
        {
            if (dp == null) throw new ArgumentNullException(nameof(dp));

            dp.SetValue(ActiveProperty, value);
        }
    }
}
