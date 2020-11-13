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
    /// Interaction logic for ExamplePage.xaml
    /// </summary>
    public partial class ExamplePage : Page
    {
        private Window _parent;
        public Window ParentWindow
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public ExamplePage()
        {
            InitializeComponent();
        }
    }
}
