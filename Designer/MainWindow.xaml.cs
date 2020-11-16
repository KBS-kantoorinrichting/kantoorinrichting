using Designer.View;
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

namespace Designer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItemNavigateExamplePage(object sender, RoutedEventArgs e)
        {
            ExamplePage page = new ExamplePage();
            this.Container.Content = page;
            page.ParentWindow = this;
        }

        private void MenuItemNavigateAddDesignPage(object sender, RoutedEventArgs e)
        {
            AddDesign page = new AddDesign();
            this.Container.Content = page;
        }

        private void ExitApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemNavigateViewProductPage(object sender, RoutedEventArgs e)
        {
            ViewProductsView page = new ViewProductsView();
            this.Container.Content = page;
        }
    }
}
