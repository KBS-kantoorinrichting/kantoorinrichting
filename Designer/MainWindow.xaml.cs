using System.Windows;

namespace Designer {
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() { InitializeComponent(); }
    }
    
    private void MenuItemNavigateViewProductPage(object sender, RoutedEventArgs e)
    {
    ViewProductsView page = new ViewProductsView();
        this.Container.Content = page;
    }
}