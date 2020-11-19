using Designer.ViewModel;
using System.Windows.Controls;

namespace Designer.View {
    /// <summary>
    /// Interaction logic for RoomEditorView.xaml
    /// </summary>
    public partial class RoomEditorView : Page {
        public RoomEditorView() {
            //pagina initializen
            InitializeComponent();
        }

        private void RadioButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void Btn1_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            RoomEditorViewModel.Template = 0;
        }
        private void Btn2_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            RoomEditorViewModel.Template = 1;
        }
    }
}