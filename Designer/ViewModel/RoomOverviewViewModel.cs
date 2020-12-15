using Designer.Other;
using Designer.View;
using Models;
using Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Polygon = System.Windows.Shapes.Polygon;

namespace Designer.ViewModel
{
    class RoomOverviewViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public BasicCommand GotoRoomTemplate { get; set; }
        public BasicCommand GotoRoomEditor { get; set; }
        public ArgumentCommand<int> DeleteCommand { get; set; }
        public ArgumentCommand<int> UpdateCommand { get; set; }
        public Dictionary<Room, Canvas> Rooms { get; set; }
        public SnackbarMessageQueue MessageQueue { get; set; }
        public RoomOverviewViewModel()
        {
            GotoRoomTemplate = new PageCommand(() => new RoomTemplateView());
            GotoRoomEditor = new PageCommand(() => new RoomEditorView());
            DeleteCommand = new ArgumentCommand<int>(DeleteRoom);
            UpdateCommand = new ArgumentCommand<int>(UpdateRoom);
            MessageQueue = new SnackbarMessageQueue();
            Reload();
        }


        public void Reload()
        { 
            // Reload de items zodat de juiste te zien zijn
            Rooms = LoadRooms();
            OnPropertyChanged();
        }

        public Dictionary<Room, Canvas> LoadRooms()
        {
            Dictionary<Room, Canvas> dictionary = new Dictionary<Room, Canvas>();
            var rooms = RoomService.Instance.GetAll();

            foreach (Room room in rooms)
            {
                dictionary.Add(room, CreateRoomCanvas(room));
            }

            return dictionary;
        }

        public Canvas CreateRoomCanvas(Room room)
        {
            int max = 0;
            Canvas canvas = new Canvas();
            List<Position> coordinates = Room.ToList(room.Positions);

            PointCollection points = new PointCollection();
            for (int i = 0; i < coordinates.Count; i++)
            {
                max = coordinates[i].X > max ? coordinates[i].X : max;
                points.Add(new Point(coordinates[i].X, coordinates[i].Y));
            }

            Polygon roomPolygon = new Polygon
            {
                Stroke = Brushes.Black,
                Fill = Brushes.Gray,
                StrokeThickness = 1,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Points = points
            };

            // Zet de dimensies van de ruimte polygon
            roomPolygon.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            canvas.Children.Add(roomPolygon);

            double scale = 270.00 / max;
            canvas.RenderTransform = new ScaleTransform(scale, scale);

            return canvas;
        }

        public void DeleteRoom(int id)
        {
            Room room = RoomService.Instance.Get(id);
            if (room == null) return;

            RoomService.Instance.Delete(room);
            MessageQueue.Enqueue("Kamer is verwijderd!");
            Reload();
        }
        
        public void UpdateRoom(int id)
        {
            Room room = RoomService.Instance.Get(id);
            if (room == null) return;

            RoomEditorView edit = new RoomEditorView(room);
            Navigator.Instance.Replace(edit);
        }

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
