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
        Dictionary<int, int> GridPoints = new Dictionary<int, int>();
        public Canvas Editor { get; set; }
        public Border CanvasBorder { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ArgumentCommand<MouseEventArgs> MouseOverCommand { get; set; }
        public BasicCommand Submit { get; set; }
        private double CanvasHeight = 500;
        private double CanvasWidth = 1280;

        public RoomEditorViewModel()
        {
            //MouseOverCommand = new ArgumentCommand<MouseEventArgs>(e => MouseMove(e.OriginalSource, e));
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
                    Editor.Children.Add(rectangle);
                    GridPoints.Add(row, column);
                }
            }
        }
        public void MouseMove(object sender, MouseEventArgs e)
        {
            
            //if (e.GetPositionds)
            { // Wanneer hij in een vakje is:
                
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Fill = System.Windows.Media.Brushes.LightBlue;
                rectangle.Width = 25;
                rectangle.Height = 25;
                rectangle.Stroke = System.Windows.Media.Brushes.Black;
                Canvas.SetTop(rectangle, (Math.Floor(e.GetPosition(Editor).Y)));
                Canvas.SetLeft(rectangle, (Math.Floor(e.GetPosition(Editor).X)));
                Editor.Children.Add(rectangle);
                // Nieuwe rectangle met grootte 25x25 in lichtblauw met zwarte randen op positie van de cursor

            }
        }

    }
}

