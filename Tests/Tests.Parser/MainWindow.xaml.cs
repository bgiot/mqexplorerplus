using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dotc.MQExplorerPlus.Application.Models.Parser.Configuration;

namespace Tests.Parser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            string test = "<parser>" +
                               "<parts>" +
                               "<part id=\"part01\"></part>" +
                               "</parts>" +
                               "<message />" +
                          "</parser>";

            var conf = ParserConfiguration.LoadFromString(test);
        }
    }
}
