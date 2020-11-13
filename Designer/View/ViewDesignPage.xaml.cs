using Designer.Model;
using Designer.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Designer.View
{
    /// <summary>
    /// Interaction logic for ViewDesignPage.xaml
    /// </summary>
    public partial class ViewDesignPage : Page
    {
        public Design Design;

        public Window ParentWindow { get; set; }
        public ViewDesignPage(Design Design)
        {
            this.Design = Design;
            InitializeComponent();
        }
    }
}
