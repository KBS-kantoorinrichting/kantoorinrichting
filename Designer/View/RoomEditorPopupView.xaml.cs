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

namespace Designer.View {
    /// <summary>
    /// Interaction logic for RoomEditorPopupView.xaml
    /// </summary>
    public partial class RoomEditorPopupView : Window {
        public RoomEditorPopupView(string text) {
            InitializeComponent();
            TextLabel.Content = text;
        }

        private void Continue_Button_Click(object sender, RoutedEventArgs e) { Close(); }
    }
}