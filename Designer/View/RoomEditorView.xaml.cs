using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
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
using Designer.Model;
using Designer.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace Designer.View {
    /// <summary>
    /// Interaction logic for RoomEditorView.xaml
    /// </summary>
    public partial class RoomEditorView : Page {
        public RoomEditorView() {
            //pagina initializen
            InitializeComponent();
        }

        private void Length_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Width_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Name_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}