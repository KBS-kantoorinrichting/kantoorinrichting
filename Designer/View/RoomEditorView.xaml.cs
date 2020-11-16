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

namespace Designer.View
{
    /// <summary>
    /// Interaction logic for RoomEditorView.xaml
    /// </summary>
    public partial class RoomEditorView : Page
    {
        // link met viewmodel
        private RoomEditorViewModel ViewModel
        {
            get { return this.DataContext as RoomEditorViewModel; }
        }

        // vorige scherm ophalen
        private Window _parent;
        public Window ParentWindow
        {
            get { return _parent; }
            set { _parent = value; }
        }
        public RoomEditorView()
        {
            //pagina initializen
            InitializeComponent();
        }

        // Voorkomt dat je text kan plakken
        private void WidthLengthTypeCheck(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!ViewModel.IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        

        // extra beveiliging of de text niet te lang is
        private void RoomNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RoomNameTextBox.Text.Length > 300)
            {
                TemplateLabel.Focus();
                RoomNameTextBox.Text = "De teksts is te lang!!";
            }
        }

         // controles of de input wel int is
        private void RoomWidthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!ViewModel.IsTextAllowed(RoomWidthTextBox.Text)) 
            {
                RoomWidthTextBox.Text = "";
            }
        }
        private void RoomLengthTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!ViewModel.IsTextAllowed(RoomLengthTextBox.Text)) 
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
                // opslaan van de ruimte als het aan de condities voldoet
                if (ViewModel.SaveRoom(RoomNameTextBox.Text, Int32.Parse(RoomWidthTextBox.Text), Int32.Parse(RoomLengthTextBox.Text)))
                {
                    //opent successvol dialoog
                    RoomEditorPopupView popup = new RoomEditorPopupView("De kamer is opgeslagen!");
                    popup.ShowDialog();
                }
                else
                {
                    //opent onsuccesvol dialoog
                    RoomEditorPopupView popup = new RoomEditorPopupView("Er is iets misgegaan! probeer opnieuw.");
                    popup.ShowDialog();
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // TODO preset knoppen
        }
    }
}
