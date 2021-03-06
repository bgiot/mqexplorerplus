﻿#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.Windows;
using System.Windows.Controls;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.Wpf.Controls.XmlEditor.CodeCompletion;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for ParsingEditorView.xaml
    /// </summary>

    public partial class ParsingEditorView : UserControl, IParsingEditorView
    {
        public ParsingEditorView()
        {
            InitializeComponent();
            uxXmlEditor.CodeCompletion = new XmlCodeCompletionBinding("Dotc.MQExplorerPlus.Core.Models.Parser.Configuration.ParserSchema.xsd;Dotc.MQExplorerPlus.Core");
        }

        private void uxCheckXml_OnClick(object sender, RoutedEventArgs e)
        {
            uxXmlEditor.ValidateXml();
        }
    }
}
