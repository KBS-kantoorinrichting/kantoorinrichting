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
using Polygon = System.Windows.Shapes.Polygon;

namespace Designer.ViewModel
{
    class RoomOverviewViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public ArgumentCommand<MouseButtonEventArgs> MouseDownCommand { get; set; }
        public BasicCommand GotoRoomTemplate { get; set; }
        public BasicCommand GotoRoomEditor { get; set; }
        public ArgumentCommand<int> DeleteCommand { get; set; }
        public ArgumentCommand<int> UpdateCommand { get; set; }
        public BasicCommand Products { get; set; }

        public List<Room> Rooms { get; set; }
        public Dictionary<Room, Canvas> AllRooms { get; set; }
        public Room SelectedRoom { get; set; }

        public Canvas Editor { get; set; }
        public ArgumentCommand<SizeChangedEventArgs> ResizeCommand { get; set; }
        public System.Windows.Shapes.Polygon RoomPoly { get; set; }
        public double Scale = 1.0;
        private double _canvasHeight = 450;
        private double _canvasWidth = 500;

        public RoomOverviewViewModel()
        {

            // binding voor de knop die je naar de room editor pagina brengt
            GotoRoomTemplate = new PageCommand(() => new RoomTemplateView());
            GotoRoomEditor = new PageCommand(() => new RoomEditorView());

            // binding voor het weergeven van het object waar je op klikt
            MouseDownCommand = new ArgumentCommand<MouseButtonEventArgs>(e => MouseDown(e.OriginalSource, e));
            // maakt nieuw canvas aan
            Editor = new Canvas();
            RoomPoly = new System.Windows.Shapes.Polygon();
            DeleteCommand = new ArgumentCommand<int>(DeleteRoom);
            UpdateCommand = new ArgumentCommand<int>(UpdateRoom);
            ResizeCommand = new ArgumentCommand<SizeChangedEventArgs>(e => ResizePage(e.OriginalSource, e));
            // herlaad pagina
            Reload();
        }


        public void Reload()
        { // Reload de items zodat de juiste te zien zijn
            Rooms = LoadItems();
            AllRooms = LoadRooms();
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

        public Dictionary<Room, Canvas> LoadRooms()
        {
            Dictionary<Room, Canvas> dictionary = new Dictionary<Room, Canvas>();
            // Linq om te zorgen dat de lijst gevuld wordt met de database content.
            Rooms = RoomService.Instance.GetAll();

            foreach (Room room in Rooms)
            {
                dictionary.Add(room, CreateRoomCanvas(room));
            }

            // this.Products is de lijst met producten
            // context.Products is de table Products van de database 
            return dictionary;
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
                SetRoomScale();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedRoom"));
            }
        }


        public void SelectRoom(int id)
        {
            // Zet het geselecteerde product op basis van het gegeven ID
            Room room = Rooms.FirstOrDefault(p => p.Id == id);
            SelectedRoom = room;
        }

        public Canvas CreateRoomCanvas(Room room)
        {
            Canvas canvas = new Canvas();
            List<Position> coordinates = Room.ToList(room.Positions);

            PointCollection points = new PointCollection();
            for (int i = 0; i < coordinates.Count; i++)
            {
                points.Add(new Point(coordinates[i].X, coordinates[i].Y));
            }

            Polygon roomPolygon = new Polygon
            {
                Height = 120,
                Width = 300,
                Stroke = Brushes.Black,
                Fill = Brushes.LightGray,
                StrokeThickness = 1,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Points = points
            };
            canvas.Children.Add(roomPolygon);
            return canvas;
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
        public void DeleteRoom(int id)
        {
            Room room = RoomService.Instance.Get(id);
            if (room == null) return;

            RoomService.Instance.Delete(room);
            new GeneralPopup("Kamer is verwijderd!").ShowDialog();
        
            Reload();
        } 
        public void UpdateRoom(int id)
        {
            Room room = RoomService.Instance.Get(id);
            if (room == null) return;

            RoomEditorView edit = new RoomEditorView(room);
            Navigator.Instance.Replace(edit);
        }

        private void ScaleCanvas(double scale)
        {
            // Kijkt of de gegeven schaal binnen de pagina past, zo niet veranderd de schaal niet
            //if (scale >= 0.01 && RoomPoly.ActualHeight * scale <= _canvasHeight && RoomPoly.ActualWidth * scale <= _canvasWidth)
            if (scale >= 0.01)
            {
                Scale = scale;
                Editor.RenderTransform = new ScaleTransform(scale, scale);
            }
        }


        public void ResizePage(object sender, SizeChangedEventArgs e)
        {
            // Berekent voor de hoogte en breedte het canvas, de hoogte en breedte veranderd alleen als de room polygon kleiner wordt dan dat deze was 
            double width = _canvasWidth / RoomPoly.ActualWidth < Scale ? _canvasWidth / RoomPoly.ActualWidth : Scale;
            double height = _canvasHeight / RoomPoly.ActualHeight < Scale ? _canvasHeight / RoomPoly.ActualHeight : Scale;

            // De kleinste waarde wordt meegegeven aan de scale functie
            ScaleCanvas(width > height ? height : width);
        }
        public void SetRoomScale()
        {
            double scale;

            // Zet de dimensies van de ruimte polygon
            RoomPoly.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            // Als de breedte hoger is dan de breedte wordt de breedte gebruikt voor de schaal en vice versa
            if (RoomPoly.DesiredSize.Width > RoomPoly.DesiredSize.Height)
            {
                scale = _canvasWidth / RoomPoly.DesiredSize.Width;
            }
            else
            {
                scale = _canvasHeight / RoomPoly.DesiredSize.Height;
            }
            ScaleCanvas(scale);
        }
        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
