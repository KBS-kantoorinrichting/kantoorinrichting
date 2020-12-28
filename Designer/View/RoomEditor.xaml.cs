using System.Windows.Controls;
using Designer.ViewModel;
using Models;

namespace Designer.View {
    /// <summary>
    ///     Interaction logic for RoomEditorView.xaml
    /// </summary>
    public partial class RoomEditorView : Page {
        public RoomEditorView() { InitializeComponent(); }

        public RoomEditorView(Room selectedRoom) {
            InitializeComponent();
            ViewModel.SetSelectedRoom(selectedRoom);
        }

        private RoomEditor ViewModel => DataContext as RoomEditor;
    }
}