using Designer.Other;
using Designer.ViewModel;
using System.Windows.Controls;

namespace Designer.View {
    /// <summary>
    /// Interaction logic for RoomEditorView.xaml
    /// </summary>
    public partial class RoomEditorView : Page {

        public BasicCommand GotoRoomTemplate { get; set; }
        public RoomEditorView() {
            //pagina initializen

            InitializeComponent();

            GotoRoomTemplate = new PageCommand(() => new RoomOverview());

        }



    }
}