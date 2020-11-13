using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Designer.View
{
    /// <summary>
    /// Interaction logic for RoomEditorView.xaml
    /// </summary>
    public partial class RoomEditorView : Page
    {
        private RoomEditorViewModel ViewModel
        {
            get { return this.DataContext as RoomEditorViewModel; }
        }

        // gebruik regex om te kijken of je text gebruikt mag worden (voor lengte en breedte)
        private static readonly Regex _regex = new Regex("[^0-9.-]+");
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        // zou moeten voorkomen dat je text kan plakken
        private void WidthLengthTypeCheck(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private Window _parent;
        public Window ParentWindow
        {
            get { return _parent; }
            set { _parent = value; }
        }
        public RoomEditorView()
        {
            InitializeComponent();
        }

        private void RoomNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
         // controleer of de input wel int is
        private void RoomWidthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsTextAllowed(RoomWidthTextBox.Text)) 
            {
                RoomWidthTextBox.Text = "";
            }
        }
        private void RoomLengthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsTextAllowed(RoomLengthTextBox.Text)) 
            {
                RoomLengthTextBox.Text = "";
            }
        }
        

        // sla de gegevens op (checkt de waardes en stuurt ze)
        private void SaveRoomButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO waardes checken
            //controleert of de waardes te parsen zijn naar int (kijken of het getallen zijn)
            long number1 = 0;
            long number2 = 0;
            bool canConvert1 = long.TryParse(RoomWidthTextBox.Text, out number1);
            bool canConvert2 = long.TryParse(RoomLengthTextBox.Text, out number2);
            if (!canConvert1)
            {
                RoomWidthTextBox.Text = "Voer aub een getal in";
            }
            else if (!canConvert2)
            {
                RoomLengthTextBox.Text = "Voer aub een getal in";
            }
            else
            {
                ViewModel.SaveRoom(RoomNameTextBox.Text, Int32.Parse(RoomWidthTextBox.Text), Int32.Parse(RoomLengthTextBox.Text));
            }

            
        }


    }
}
