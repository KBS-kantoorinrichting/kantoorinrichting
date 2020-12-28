using Designer.ViewModel;
using Models;
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
    /// Interaction logic for RoomEditorView.xaml
    /// </summary>
    public partial class RoomEditorView : Page
    {
        private RoomEditor ViewModel => DataContext as RoomEditor;


        public RoomEditorView()
        {
            InitializeComponent();
        }

        public RoomEditorView(Room selectedRoom)
        {
            InitializeComponent();
            ViewModel.SetSelectedRoom(selectedRoom);
       
            
        }
    }
}
