using Designer.Other;
using Designer.View;
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Designer.ViewModel
{
    public class RoomEditorViewModel : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public List<Line> GridLines = new List<Line>();
        public List<System.Windows.Point> Points = new List<System.Windows.Point>();
        public Dictionary<System.Windows.Point, System.Windows.Shapes.Rectangle> RectangleDictionary = new Dictionary<System.Windows.Point, System.Windows.Shapes.Rectangle>();
        public Canvas Editor { get; set; }
        public Border CanvasBorder { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ArgumentCommand<MouseEventArgs> MouseOverCommand { get; set; }
        public BasicCommand Submit { get; set; }
        private double CanvasHeight = 500;
        private double CanvasWidth = 1280;

        public RoomEditorViewModel()
        {
            MouseOverCommand = new ArgumentCommand<MouseEventArgs>(e => MouseMove(e.OriginalSource, e));
            Editor = new Canvas();
            Reload();
        }
        public void Reload()
        { // Reload de items zodat de juiste te zien zijn
            Editor.Children.Clear();
            DrawGrid();
            OnPropertyChanged();
        }
        private void OnPropertyChanged(string propertyName = "")
        {
            // herlaad de hele pagina
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void SubmitRoom()
        {
            /*//if (SaveRoom(Name) != null)
            {
                //opent successvol dialoog
                GeneralPopup popup = new GeneralPopup("De kamer is opgeslagen!");
                popup.ShowDialog();
                return;
            }
            //opent onsuccesvol dialoog
            GeneralPopup popuperror = new GeneralPopup("Er is iets misgegaan! probeer opnieuw.");
            popuperror.ShowDialog();*/
        }

        public void DrawGrid()
        {
           
            /* int LengthPerSquare = (int)CanvasWidth / 25;

             for (int i = 1; i <= LengthPerSquare; i++)
             {
                 Line line = new Line();
                 line.Stroke = System.Windows.Media.Brushes.Black;
                 line.Y1 = 0;
                 line.Y2 = 700;
                 line.X1 = (i * 25);
                 line.X2 = i * 25;
                 GridLines.Add(line);
             }
             for (int i = 0; i <= LengthPerSquare; i++)
             {
                 Line line = new Line();
                 line.Stroke = System.Windows.Media.Brushes.Black;
                 line.X1 = 0;
                 line.X2 = 1280;
                 line.Y1 = (i * 25);
                 line.Y2 = i * 25;
                 GridLines.Add(line);
             }
             foreach (Line line in GridLines)
             {
                 Editor.Children.Add(line);
             }*/
            var rows = 25*25;
            var columns = 50 * 25;
            for (int row = 0; row < rows; row +=25)
            {
                
                for (int column = 0; column < columns; column+=25)
                {
                    System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                    rectangle.Fill = System.Windows.Media.Brushes.White;
                    rectangle.Width = 25;
                    rectangle.Height = 25;
                    rectangle.Stroke = System.Windows.Media.Brushes.Black;
                    Canvas.SetTop(rectangle, row);
                    Canvas.SetLeft(rectangle, column);
                    System.Windows.Point Point = new System.Windows.Point(row, column);
                    Points.Add(Point);
                    Editor.Children.Add(rectangle);
                    RectangleDictionary.Add(Point, rectangle);
                }
            }
        }
        public void MouseMove(object sender, MouseEventArgs e)
        {
            int y = Convert.ToInt32(e.GetPosition(Editor).Y);
            int x = Convert.ToInt32(e.GetPosition(Editor).X);



            if (Points.Contains(new System.Windows.Point(y, x)))
            { // Wanneer hij in een vakje is:
                Debug.WriteLine($"{y}  {x}");
                

            }
        }

    }
}

