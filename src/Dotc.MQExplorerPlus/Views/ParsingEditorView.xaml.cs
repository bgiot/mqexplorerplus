#region copyright
//  
// Copyright (c) DOT Consulting scrl. All rights reserved.  
// Licensed under the provided EULA. See EULA file in the solution root for full license information.  
//
#endregion
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Dotc.MQExplorerPlus.Core.Views;
using Dotc.Wpf.Controls.XmlEditor.CodeCompletion;

namespace Dotc.MQExplorerPlus.Views
{
    /// <summary>
    /// Interaction logic for ParsingEditorView.xaml
    /// </summary>
    [Export(typeof(IParsingEditorView)), PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class ParsingEditorView : UserControl, IParsingEditorView
    {
        public ParsingEditorView()
        {
            InitializeComponent();
            uxXmlEditor.CodeCompletion = new XmlCodeCompletionBinding("Dotc.MQExplorerPlus.Application.Models.Parser.Configuration.ParserSchema.xsd;Dotc.MQExplorerPlus.Application");
        }

        private void uxCheckXml_OnClick(object sender, RoutedEventArgs e)
        {
            uxXmlEditor.ValidateXml();
        }
    }
}
