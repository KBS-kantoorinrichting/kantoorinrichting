using Designer.Model;
using Designer.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Designer.View
{
    /// <summary>
    /// Interaction logic for ViewDesignPage.xaml
    /// </summary>
    public partial class ViewDesignPage : Page
    {
        public Window ParentWindow { get; set; }
        private ViewDesignViewModel ViewModel => DataContext as ViewDesignViewModel;

        public ViewDesignPage(Design Design)
        {
            InitializeComponent();
            ViewModel.SetDesign(Design);
        }
        private void Page_Initialized(object sender, EventArgs e)
        {
        }

    }
}
