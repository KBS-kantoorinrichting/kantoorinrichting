using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Designer.View
{
    /// <summary>
    /// Interaction logic for GeneralPopup.xaml
    /// </summary>
    public partial class GeneralPopup : Window
    {
        public GeneralPopup(string text)
        {
            InitializeComponent();
            TextLabel.Content = text;
        }

        private void Continue_Button_Click(object sender, RoutedEventArgs e) { Close(); }
    }


}

