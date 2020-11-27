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

            int LengthPerSquare = (int)CanvasWidth / 25;

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
            }
        }
        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (e.GetPosition(Editor).X % 25 == 0)
            {
                /*Rect br = new Rect();
                br.X = (int)e.GetPosition(Editor).X;
                br.Y = (int)e.GetPosition(Editor).Y;
                br.Width = 25;
                br.Height = 25;
                Editor.Children.Add(br); 
                br kan niet toegevoegd worden aan Editor.Children*/

                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                //rectangle.X= (int)e.GetPosition(Editor).X;
                //rectangle.Y = (int)e.GetPosition(Editor).Y; deze regels werken niet
                rectangle.Fill = System.Windows.Media.Brushes.LightBlue;
                rectangle.Width = 25;
                rectangle.Height = 25;
                rectangle.Stroke = System.Windows.Media.Brushes.Black;
                Editor.Children.Add(rectangle);

                // Nu tekent hij een blauw 25*25 vierkantje op 0,0 wanneer de muis beweegt

            }
        }

    }
}

