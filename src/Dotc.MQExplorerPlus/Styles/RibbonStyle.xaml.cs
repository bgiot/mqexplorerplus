#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using Dotc.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Shapes;

namespace Dotc.MQExplorerPlus.Styles
{
    public partial class RibbonStyle
    {
        private void RibbonLoaded(object sender, RoutedEventArgs e)
        {

            Ribbon ribbon = (Ribbon)sender;

            var child = VisualTreeAssist.FindChild<Grid>(ribbon);//  VisualTreeHelper.GetChild((DependencyObject)sender, 0) as Grid;
            if (child != null)
            {
                child.RowDefinitions[0].Height = new GridLength(0, System.Windows.GridUnitType.Pixel);
                child.RowDefinitions[1].Height = new GridLength(0, System.Windows.GridUnitType.Pixel);
            }

            var lines = VisualTreeAssist.GetChilds<Line>(ribbon);
            if (lines != null)
                foreach (Line line in lines)
                    line.Visibility = Visibility.Collapsed;
        }
    }
}
