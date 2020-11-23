using Designer.ViewModel;
using System.Windows.Controls;
using Models;

namespace Designer.View {
    /// <summary>
    /// Interaction logic for ViewDesignPage.xaml
    /// </summary>
    public partial class ViewDesignPage : Page {
        private ViewDesignViewModel ViewModel => DataContext as ViewDesignViewModel;

        public ViewDesignPage(Design design) {
            InitializeComponent();
            ViewModel.SetDesign(design);
        }
    }
}