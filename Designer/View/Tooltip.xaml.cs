using Designer.Model;
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
using System.Windows.Shapes;

namespace Designer.View
{
    /// <summary>
    /// Interaction logic for Tooltip.xaml
    /// </summary>
    public partial class Tooltip : Window
    {
        public Product Product { get; set; }
        public Tooltip(Product Product)
        {
            InitializeComponent();
        }
    }
}
