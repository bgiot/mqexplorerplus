#region License
/*--------------------------------------------------------------------------------
    Copyright (c) 2011-2013 David Wendland

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
--------------------------------------------------------------------------------*/
#endregion License

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Dotc.Wpf.Controls
{
    /// <summary>
    /// Helps you for finding objects inside the visual tree
    /// </summary>
    /// <example>
    /// <code lang="XAML">
    /// <![CDATA[
    /// <UserControl x:Class="DW.SharpTools.Demo.VisualTreeAssistControl"
    ///              xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///              xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///              xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
    ///              xmlns:d="http://schemas.microsoft.com/expression/blend/2008" 
    ///              mc:Ignorable="d" 
    ///              d:DesignHeight="300" d:DesignWidth="300">
    ///     <Grid HorizontalAlignment="Center" VerticalAlignment="Top">
    ///         <Button Content="Find Owner Window" Click="Button_Click" />
    ///     </Grid>
    /// </UserControl>]]>
    /// </code>
    /// <code lang="cs">
    /// <![CDATA[
    /// using System.Windows;
    /// using System.Windows.Controls;
    /// 
    /// namespace DW.SharpTools.Demo
    /// {
    ///     public partial class VisualTreeAssistControl : UserControl
    ///     {
    ///         public VisualTreeAssistControl()
    ///         {
    ///             InitializeComponent();
    ///         }
    /// 
    ///         private void Button_Click(object sender, RoutedEventArgs e)
    ///         {
    ///             var ownerWindow = VisualTreeAssist.FindParent<Window>(sender);
    ///             if (ownerWindow != null)
    ///                 MessageBox.Show(string.Format("Owner window '{0}' found", ownerWindow.Title));
    ///         }
    ///     }
    /// }]]>
    /// </code>
    /// </example>
    public static class VisualTreeAssist
    {
        #region Parent
        /// <summary>
        /// Tries to find a parent object by the type
        /// </summary>
        /// <typeparam name="TParentType">The type of the object</typeparam>
        /// <param name="item">The child control</param>
        /// <returns>The found parent object. If nothing is found the default is return</returns>
        public static TParentType FindParent<TParentType>(object item) where TParentType : DependencyObject
        {
            var child = item as DependencyObject;
            if (child == null)
                return default(TParentType);

            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is TParentType))
                parent = VisualTreeHelper.GetParent(parent);

            return parent != null ? (TParentType)parent : default(TParentType);
        }

        /// <summary>
        /// Tries to find a parent object by its name
        /// </summary>
        /// <param name="item">The child control</param>
        /// <param name="name">The name of the object</param>
        /// <returns>The found parent object. If nothing is found the default is null</returns>
        public static object FindNamedParent(object item, string name)
        {
            var child = item as DependencyObject;
            if (child == null ||
                string.IsNullOrWhiteSpace(name))
                return null;

            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (HasName(parent, name))
                    return parent;
                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }

        /// <summary>
        /// Tries to find a parent object by the type and name
        /// </summary>
        /// <typeparam name="TParentType">The type of the object</typeparam>
        /// <param name="item">The child control</param>
        /// <param name="name">The name of the object</param>
        /// <returns>The found parent object. If nothing is found the default is return</returns>
        public static TParentType FindNamedParent<TParentType>(object item, string name) where TParentType : DependencyObject
        {
            var foundItem = FindNamedParent(item, name);
            return foundItem is TParentType ? (TParentType)foundItem : default(TParentType);
        }

        /// <summary>
        /// Gets all parents by the given type
        /// </summary>
        /// <typeparam name="TParentType">The type of the objects</typeparam>
        /// <param name="item">The child control</param>
        /// <returns>A list of the parent controls. If nothing is found the list is empty</returns>
        public static List<TParentType> GetParents<TParentType>(object item) where TParentType : DependencyObject
        {
            var parents = new List<TParentType>();
            var parent = FindParent<TParentType>(item);
            while (parent != null)
            {
                parents.Add(parent);
                parent = FindParent<TParentType>(parent);
            }
            return parents;
        }

        /// <summary>
        /// Gets the count of all parents by the given type
        /// </summary>
        /// <typeparam name="TParentType">The type of the objects</typeparam>
        /// <param name="item">The child control</param>
        /// <returns>The count how many parents by the type are found</returns>
        public static int GetParentCount<TParentType>(object item) where TParentType : DependencyObject
        {
            return GetParents<TParentType>(item).Count;
        }

        /// <summary>
        /// Gets all parents by the given type until the parent is the end type
        /// </summary>
        /// <typeparam name="TParentType">The type of the objects</typeparam>
        /// <typeparam name="TEndType">The type for cancel the search</typeparam>
        /// <param name="item">The child control</param>
        /// <returns>A list of the parent controls. If nothing is found the list is empty</returns>
        public static List<TParentType> GetParentsUntil<TParentType, TEndType>(object item)
            where TParentType : DependencyObject
            where TEndType : DependencyObject
        {
            var parents = new List<TParentType>();

            var child = item as DependencyObject;
            if (child == null)
                return parents;

            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null && !(parent is TEndType))
            {
                if (parent is TParentType)
                    parents.Add((TParentType)parent);
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parents;
        }

        /// <summary>
        /// Gets the count of all parents by the given type until the parent is the end type
        /// </summary>
        /// <typeparam name="TParentType">The type of the objects</typeparam>
        /// <typeparam name="TEndType">The type for cancel the search</typeparam>
        /// <param name="item">The child control</param>
        /// <returns>The count how many parents by the type are found</returns>
        public static int GetParentsUntilCount<TParentType, TEndType>(object item)
            where TParentType : DependencyObject
            where TEndType : DependencyObject
        {
            return GetParentsUntil<TParentType, TEndType>(item).Count;
        }
        #endregion Parent

        #region Child
        /// <summary>
        /// Tries to find a child object by the type
        /// </summary>
        /// <typeparam name="TChildType">The type of the object</typeparam>
        /// <param name="item">The child control</param>
        /// <returns>The found child object. If nothing is found the default is return</returns>
        public static TChildType FindChild<TChildType>(object item) where TChildType : DependencyObject
        {
            var parent = item as DependencyObject;
            if (parent == null)
                return default(TChildType);

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; ++i)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TChildType)
                    return (TChildType)child;
                
                var foundChild = FindChild<TChildType>(child);
                if (foundChild != null)
                    return foundChild;
            }

            return default(TChildType);
        }

        /// <summary>
        /// Tries to find a child object by the name
        /// </summary>
        /// <param name="item">The child control</param>
        /// <param name="name">The name of the object</param>
        /// <returns>The found child object. If nothing is found the default is null</returns>
        public static object FindNamedChild(object item, string name)
        {
            var parent = item as DependencyObject;
            if (parent == null ||
                string.IsNullOrWhiteSpace(name))
                return null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; ++i)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (HasName(child, name))
                    return child;
                
                var foundChild = FindNamedChild(child, name);
                if (foundChild != null)
                    return foundChild;
            }

            return null;
        }

        /// <summary>
        /// Tries to find a child object by the type and name
        /// </summary>
        /// <typeparam name="TChildType">The type of the object</typeparam>
        /// <param name="item">The child control</param>
        /// <param name="name">The name of the object</param>
        /// <returns>The found child object. If nothing is found the default is return</returns>
        public static TChildType FindNamedChild<TChildType>(object item, string name) where TChildType : DependencyObject
        {
            var foundItem = FindNamedChild(item, name);
            return foundItem is TChildType ? (TChildType)foundItem : default(TChildType);
        }

        /// <summary>
        /// Gets all childs by the given type
        /// </summary>
        /// <typeparam name="TChildType">The type of the object</typeparam>
        /// <param name="item">The child control</param>
        /// <returns>A list of the child controls. If nothing is found the list is empty</returns>
        public static List<TChildType> GetChilds<TChildType>(object item) where TChildType : DependencyObject
        {
            var childs = new List<TChildType>();
            GetChilds(item, childs);
            return childs;
        }

        private static void GetChilds<TChildType>(object item, ICollection<TChildType> target) where TChildType : DependencyObject
        {
            var parent = item as DependencyObject;
            if (parent == null)
                return;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; ++i)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is TChildType)
                    target.Add((TChildType)child);
                GetChilds(child, target);
            }
        }

        /// <summary>
        /// Gets the count of all childs by the given type
        /// </summary>
        /// <typeparam name="TChildType">The type of the object</typeparam>
        /// <param name="item">The child control</param>
        /// <returns>The count how many childs by the type are found</returns>
        public static int GetChildsCount<TChildType>(object item) where TChildType : DependencyObject
        {
            return GetChilds<TChildType>(item).Count;
        }
        #endregion Child

        [ExcludeFromCodeCoverage] // Cannot create a DependencyObject in the visual tree which is not a FrameworkElement
        private static bool HasName(DependencyObject item, string name)
        {
            return item is FrameworkElement &&
                   ((FrameworkElement)item).Name == name;
        }
    }
}
