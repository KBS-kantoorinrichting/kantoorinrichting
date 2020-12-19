using Designer.ViewModel;
using System.Windows.Controls;
using Models;

namespace Designer.View {
    /// <summary>
    /// Interaction logic for ViewDesignPage.xaml
    /// </summary>
    public partial class ViewDesignPage : Page {
        private DesignEditor ViewModel => DataContext as DesignEditor;

        public ViewDesignPage(Design design) {
            InitializeComponent();
            ViewModel.SetDesign(design);
        }
    }
}