using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Dotc.Wpf
{
    /// <summary>
    /// Fournit des méthodes d'extension pour les DependencyObjects
    /// </summary>
    public static class DependencyObjectExtensions
    {


        /// <summary>
        /// Renvoie la valeur de la DependencyProperty spécifiée pour cet objet, en la convertissant dans le type souhaité
        /// </summary>
        /// <typeparam name="T">Le type de retour souhaité</typeparam>
        /// <param name="obj">Le DependencyObject dont on veut obtenir une valeur de propriété</param>
        /// <param name="dp">La DependencyProperty dont on veut obtenir la valeur pour cet objet</param>
        /// <returns>La valeur de la propriété pour cet objet</returns>
        public static T GetValue<T>(this DependencyObject obj, DependencyProperty dp)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return (T)obj.GetValue(dp);
        }

        /// <summary>
        /// Renvoie le parent de l'objet courant dans l'arbre logique
        /// </summary>
        /// <param name="obj">L'objet dont on veut obtenir le parent</param>
        /// <returns>Le parent de l'objet dans l'arbre logique</returns>
        public static DependencyObject GetLogicalParent(this DependencyObject obj)
        {
            return LogicalTreeHelper.GetParent(obj);
        }

        /// <summary>
        /// Renvoie les enfants de l'objet courant dans l'arbre logique
        /// </summary>
        /// <param name="obj">L'objet dont on veut obtenir les enfants</param>
        /// <returns>Les enfants de l'objet dans l'arbre logique</returns>
        public static IEnumerable<DependencyObject> GetLogicalChildren(this DependencyObject obj)
        {
            return LogicalTreeHelper.GetChildren(obj).Cast<DependencyObject>();
        }

        /// <summary>
        /// Renvoie le parent de l'objet courant dans l'arbre visuel
        /// </summary>
        /// <param name="obj">L'objet dont on veut obtenir le parent</param>
        /// <returns>Le parent de l'objet dans l'arbre visuel</returns>
        public static DependencyObject GetVisualParent(this DependencyObject obj)
        {
            return VisualTreeHelper.GetParent(obj);
        }

        /// <summary>
        /// Renvoie les enfants de l'objet courant dans l'arbre visuel
        /// </summary>
        /// <param name="obj">L'objet dont on veut obtenir les enfants</param>
        /// <returns>Les enfants de l'objet dans l'arbre visuel</returns>
        public static IEnumerable<DependencyObject> GetVisualChildren(this DependencyObject obj)
        {
            var count = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < count; i++)
            {
                yield return VisualTreeHelper.GetChild(obj, i);
            }
        }

        /// <summary>
        /// Remonte l'arbre visuel à la recherche d'un parent du type spécifié
        /// </summary>
        /// <typeparam name="T">Le type du parent recherché</typeparam>
        /// <param name="obj">L'objet dont on cherche le parent</param>
        /// <returns>Le premier parent du type spécifié rencontré, ou null si un tel parent n'existe pas</returns>
        public static T FindAncestor<T>(this DependencyObject obj) where T : DependencyObject
        {
            var tmp = VisualTreeHelper.GetParent(obj);
            while (tmp != null && !(tmp is T))
            {
                tmp = VisualTreeHelper.GetParent(tmp);
            }
            return tmp as T;
        }

        /// <summary>
        /// Remonte l'arbre visuel à la recherche d'un parent du type spécifié
        /// </summary>
        /// <param name="obj">L'objet dont on cherche le parent</param>
        /// <param name="ancestorType">Le type du parent recherché</param>
        /// <returns>Le premier parent du type spécifié rencontré, ou null si un tel parent n'existe pas</returns>
        public static DependencyObject FindAncestor(this DependencyObject obj, Type ancestorType)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            if (ancestorType == null) throw new ArgumentNullException(nameof(ancestorType));

            var tmp = VisualTreeHelper.GetParent(obj);
            while (tmp != null && !ancestorType.IsInstanceOfType(tmp))
            {
                tmp = VisualTreeHelper.GetParent(tmp);
            }
            return tmp;
        }

        /// <summary>
        /// Parcours l'arbre visuel d'un élément et renvoie tous ses descendants du type spécifié.
        /// </summary>
        /// <typeparam name="T">Type des descendants recherchés</typeparam>
        /// <param name="obj">Elément à partir duquel effectuer la recherche</param>
        /// <returns>Séquence de descendants du type spécifié</returns>
        public static IEnumerable<T> FindDescendants<T>(this DependencyObject obj) where T : DependencyObject
        {
            var queue = new Queue<DependencyObject>(obj.GetVisualChildren());
            while (queue.Count > 0)
            {
                var child = queue.Dequeue();
                var descendants = child as T;
                if (descendants != null)
                    yield return descendants;
                foreach (var c in child.GetVisualChildren())
                {
                    queue.Enqueue(c);
                }
            }
        }

        /// <summary>
        /// Parcours l'arbre visuel d'un élément et renvoie tous ses descendants du type spécifié.
        /// </summary>
        /// <param name="obj">Elément à partir duquel effectuer la recherche</param>
        /// <param name="descendantType">Type des descendants recherchés</param>
        /// <returns>Séquence de descendants du type spécifié</returns>
        public static IEnumerable<DependencyObject> FindDescendants(this DependencyObject obj, Type descendantType)
        {
            var queue = new Queue<DependencyObject>(obj.GetVisualChildren());
            while (queue.Count > 0)
            {
                var child = queue.Dequeue();
                if (descendantType.IsInstanceOfType(child))
                    yield return child;
                foreach (var c in child.GetVisualChildren())
                {
                    queue.Enqueue(c);
                }
            }
        }

        /// <summary>
        /// Ajoute un handler pour être notifié du changement de la DependencyProperty spécifiée
        /// </summary>
        /// <param name="obj">L'objet à surveiller</param>
        /// <param name="property">La propriété à surveiller</param>
        /// <param name="handler">Le handler à appeler quand la valeur de la propriété change</param>
        public static void AddPropertyChangedHandler(this DependencyObject obj, DependencyProperty property, EventHandler handler)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var desc = DependencyPropertyDescriptor.FromProperty(property, obj.GetType());
            desc.AddValueChanged(obj, handler);
        }

        /// <summary>
        /// Supprime un handler de notification de changement de DependencyProperty
        /// </summary>
        /// <param name="obj">L'objet qui possède la propriété</param>
        /// <param name="property">La propriété</param>
        /// <param name="handler">Le handler à supprimer</param>
        public static void RemovePropertyChangedHandler(this DependencyObject obj, DependencyProperty property, EventHandler handler)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var desc = DependencyPropertyDescriptor.FromProperty(property, obj.GetType());
            desc.RemoveValueChanged(obj, handler);
        }
    }
}
