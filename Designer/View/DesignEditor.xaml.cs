using System.Windows.Controls;
using Designer.ViewModel;
using Models;

namespace Designer.View {
    /// <summary>
    ///     Interaction logic for ViewDesignPage.xaml
    /// </summary>
    public partial class DesignEditorView : Page {
        public DesignEditorView(Design design) {
            InitializeComponent();
            ViewModel.SetDesign(design);
        }

        private DesignEditor ViewModel => DataContext as DesignEditor;
    }
}