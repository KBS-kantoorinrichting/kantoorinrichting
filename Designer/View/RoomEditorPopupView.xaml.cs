using System.Windows;

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