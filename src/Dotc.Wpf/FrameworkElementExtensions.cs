using System;
using System.Windows;

namespace Dotc.Wpf
{
    /// <summary>
    /// Fournit des méthodes d'extension pour les contrôles WPF (FrameworkElement)
    /// </summary>
    public static class FrameworkElementExtensions
    {
        /// <summary>
        /// Effectue une action quand l'élément est chargé
        /// </summary>
        /// <param name="element">Element pour lequel l'action doit être effectuée</param>
        /// <param name="action">Action à effectuer</param>
        public static void DoWhenLoaded(this FrameworkElement element, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (element == null) throw new ArgumentNullException(nameof(element));

            if (element.IsLoaded)
            {
                action();
            }
            else
            {
                RoutedEventHandler handler = null;
                handler = (sender, e) =>
                              {
                                  element.Loaded -= handler;
                                  action();
                              };
                element.Loaded += handler;
            }
        }

        /// <summary>
        /// Effectue une action quand l'élément est chargé
        /// </summary>
        /// <typeparam name="T">Type de l'élément</typeparam>
        /// <param name="element">Element pour lequel l'action doit être effectuée</param>
        /// <param name="action">Action à effectuer</param>
        public static void DoWhenLoaded<T>(this T element, Action<T> action)
            where T : FrameworkElement
        {
            if (element.IsLoaded)
            {
                action(element);
            }
            else
            {
                RoutedEventHandler handler = null;
                handler = (sender, e) =>
                {
                    element.Loaded -= handler;
                    action(element);
                };
                element.Loaded += handler;
            }
        }
    }
}
