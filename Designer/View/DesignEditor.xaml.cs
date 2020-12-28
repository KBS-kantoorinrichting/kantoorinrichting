using Designer.ViewModel;
using System.Windows.Controls;
using Models;

namespace Designer.View {
    /// <summary>
    /// Interaction logic for ViewDesignPage.xaml
    /// </summary>
    public partial class DesignEditorView : Page {
        private DesignEditor ViewModel => DataContext as DesignEditor;

        public DesignEditorView(Design design) {
            InitializeComponent();
            ViewModel.SetDesign(design);
        }
    }
}