#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Windows;
using System.Windows.Controls;
using Dotc.MQExplorerPlus.Core.Models.Parser;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for ParsingResultView.xaml
    /// </summary>
    public partial class ParsingResultView : UserControl
    {
        public ParsingResultView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ResultProperty;

        static ParsingResultView()
        {
            ParsingResultView.ResultProperty =
                                           DependencyProperty.Register("ItemsSource",
                                      typeof(ParsingResult), typeof(ParsingResultView));
        }

        public ParsingResult ItemsSource
        {
            get
            {
                return (ParsingResult)GetValue(ParsingResultView.ResultProperty);
            }
            set
            {
                SetValue(ParsingResultView.ResultProperty, value);
            }
        }

    }
}
