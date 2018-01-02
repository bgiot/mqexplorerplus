using System;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace Dotc.Wpf
{
    public class PasswordHelper
    {
        public static readonly DependencyProperty BindablePasswordProperty =
            DependencyProperty.RegisterAttached("BindablePassword",
            typeof(SecureString), typeof(PasswordHelper),
            new FrameworkPropertyMetadata(null, OnPasswordPropertyChanged));

        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached("BindPassword",
            typeof(bool), typeof(PasswordHelper), new PropertyMetadata(false, BindPassword));

        private static readonly DependencyProperty UpdatingPasswordProperty =
            DependencyProperty.RegisterAttached("UpdatingPassword", typeof(bool),
            typeof(PasswordHelper));

        public static void SetBindPassword(DependencyObject dp, bool value)
        {
            if (dp == null) throw new ArgumentNullException(nameof(dp));

            dp.SetValue(BindPasswordProperty, value);
        }

        public static bool GetBindPassword(DependencyObject dp)
        {
            if (dp == null) throw new ArgumentNullException(nameof(dp));

            return (bool)dp.GetValue(BindPasswordProperty);
        }

        public static string GetBindablePassword(DependencyObject dp)
        {
            if (dp == null) throw new ArgumentNullException(nameof(dp));

            return (string)dp.GetValue(BindablePasswordProperty);
        }

        public static void SetBindablePassword(DependencyObject dp, SecureString value)
        {
            if (dp == null) throw new ArgumentNullException(nameof(dp));

            dp.SetValue(BindablePasswordProperty, value);
        }

        private static bool GetUpdatingPassword(DependencyObject dp)
        {
            return (bool)dp.GetValue(UpdatingPasswordProperty);
        }

        private static void SetUpdatingPassword(DependencyObject dp, bool value)
        {
            dp.SetValue(UpdatingPasswordProperty, value);
        }

        private static void OnPasswordPropertyChanged(DependencyObject sender,
        DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox == null) return;
            WeakEventManager<PasswordBox, RoutedEventArgs>
                .RemoveHandler(passwordBox, "PasswordChanged", PasswordChanged);
            if (!GetUpdatingPassword(passwordBox))
            {
                passwordBox.Password = e.NewValue as string;
            }
            WeakEventManager<PasswordBox, RoutedEventArgs>
                .AddHandler(passwordBox, "PasswordChanged", PasswordChanged);
        }

        private static void BindPassword(DependencyObject sender,
        DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox == null)
                return;
            if ((bool)e.OldValue)
            {
                WeakEventManager<PasswordBox, RoutedEventArgs>
                    .RemoveHandler(passwordBox, "PasswordChanged", PasswordChanged);
            }
            if ((bool)e.NewValue)
            {
                WeakEventManager<PasswordBox, RoutedEventArgs>
                    .AddHandler(passwordBox, "PasswordChanged", PasswordChanged);
            }
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
          var passwordBox = sender as PasswordBox;
            SetUpdatingPassword(passwordBox, true);
            if (passwordBox == null) return;
            SetBindablePassword(passwordBox, passwordBox.SecurePassword);
            SetUpdatingPassword(passwordBox, false);
        }

    }
}
