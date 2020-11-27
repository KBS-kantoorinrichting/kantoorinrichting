using Designer.Other;
using Designer.View;
using Models;
using Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Designer.ViewModel
{
    class RoomOverviewViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public ArgumentCommand<MouseButtonEventArgs> MouseDownCommand { get; set; }
        public BasicCommand GotoRoomTemplate { get; set; }
        public BasicCommand DeleteCommand { get; set; }
        public BasicCommand Products { get; set; }

        public List<Room> Rooms { get; set; }
        public Room SelectedRoom { get; set; }

        public Canvas Editor { get; set; }
        private Point _previousPosition;
        private ProductPlacement _selectedPlacement;
        private ProductPlacement _draggingPlacement;
        public Polygon RoomPoly { get; set; }

        public RoomOverviewViewModel()
        {

            // binding voor de knop die je naar de room editor pagina brengt
            GotoRoomTemplate = new PageCommand(() => new RoomEditorView());
            // binding voor het weergeven van het object waar je op klikt
            MouseDownCommand = new ArgumentCommand<MouseButtonEventArgs>(e => MouseDown(e.OriginalSource, e));
            // maakt nieuw canvas aan
            Editor = new Canvas();
            RoomPoly = new Polygon();
            DeleteCommand = new BasicCommand(Delete);
            // herlaad pagina
            Reload();
            
        }


        public void Reload()
        { // Reload de items zodat de juiste te zien zijn
            Rooms = LoadItems();
            Editor.Children.Clear();
            OnPropertyChanged();
        }

        public List<Room> LoadItems()
        {
            // Linq om te zorgen dat de lijst gevuld wordt met de database content.
            Rooms = RoomService.Instance.GetAll();

            // this.Products is de lijst met producten
            // context.Products is de table Products van de database 
            return Rooms;
        }

        public void MouseDown(object sender, MouseButtonEventArgs e)
        {

            // Linker muisknop moet ingdrukt zijn
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // obj = (Product)((Image)sender).DataContext;
                //var  = (Product)(sender).DataContext;
                //SelectedProduct = obj;
                //SelectProduct(obj.Id);
                if (sender.GetType() == typeof(TextBlock))
                {
                    var room = (Room)((TextBlock)sender).DataContext;
                    SelectRoom(room.Id);
                }
                
                SetRoomDimensions();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedRoom"));
            }
        }


        public void SelectRoom(int id)
        {
            // Zet het geselecteerde product op basis van het gegeven ID
            Room room = Rooms.FirstOrDefault(p => p.Id == id);
            SelectedRoom = room;
        }

        public void SetRoomDimensions()
        {
            Editor.Children.Clear();
            // TODO: Replace with room positions
            var coordinates = Room.ToList(SelectedRoom.Positions);



            PointCollection points = new PointCollection();
            // Voeg de punten toe aan een punten collectie
            for (int i = 0; i < coordinates.Count; i++)
            {
                points.Add(new Point(coordinates[i].X, coordinates[i].Y));
            }

            RoomPoly.Stroke = Brushes.Black;
            RoomPoly.Fill = Brushes.LightGray;
            RoomPoly.StrokeThickness = 1;
            RoomPoly.HorizontalAlignment = HorizontalAlignment.Left;
            RoomPoly.VerticalAlignment = VerticalAlignment.Center;
            RoomPoly.Points = points;
            Editor.Children.Add(RoomPoly);
           
        }
        public void Delete()
        {
            if (SelectedRoom == null || RoomService.Instance.Count() == 0)
            {
                return;
            }
            RoomService.Instance.Delete(SelectedRoom);
            GeneralPopup warning = new GeneralPopup("Kamer is verwijderd!");
            warning.ShowDialog();
            SelectedRoom = null;
        
            Reload();

        }
        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
