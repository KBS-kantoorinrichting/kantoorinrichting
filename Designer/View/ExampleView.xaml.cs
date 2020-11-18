using System.Windows;
using System.Windows.Controls;

namespace Designer.View {
    /// <summary>
    ///     Interaction logic for ExamplePage.xaml
    /// </summary>
    public partial class ExamplePage : Page {
        public Window ParentWindow { get; set; }
        
        public ExamplePage() { InitializeComponent(); }
    }
}