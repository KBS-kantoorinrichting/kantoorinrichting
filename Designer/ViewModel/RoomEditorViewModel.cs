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
        public List<System.Windows.Point> SelectedPoints = new List<System.Windows.Point>();
        public Dictionary<System.Windows.Point, System.Windows.Shapes.Rectangle> RectangleDictionary = new Dictionary<System.Windows.Point, System.Windows.Shapes.Rectangle>();
        public Canvas Editor { get; set; }
        public System.Windows.Shapes.Rectangle HoveredRectangle = new System.Windows.Shapes.Rectangle();
        public System.Windows.Point LastSelected = new System.Windows.Point();

        public Border CanvasBorder { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ArgumentCommand<MouseEventArgs> MouseOverCommand { get; set; }
        public ArgumentCommand<MouseButtonEventArgs> MouseDownCommand { get; set; }
        public BasicCommand Submit { get; set; }
        private double CanvasHeight = 500;
        private double CanvasWidth = 1280;

        public RoomEditorViewModel()
        {
            LastSelected.Y = 1000000;
            MouseOverCommand = new ArgumentCommand<MouseEventArgs>(e => MouseMove(e.OriginalSource, e));
            MouseDownCommand = new ArgumentCommand<MouseButtonEventArgs>(e => MouseClick(e.OriginalSource, e));
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
            int y = Convert.ToInt32(e.GetPosition(Editor).Y - (e.GetPosition(Editor).Y % 25));
            int x = Convert.ToInt32(e.GetPosition(Editor).X - (e.GetPosition(Editor).X % 25));
            //int x = Convert.ToInt32(e.GetPosition(Editor).X);


           /* if (HoveredRectangle != null)
            {
                if (HoveredRectangle == RectangleDictionary[new System.Windows.Point(y, x)])
                {

                }
                else
                {
                    if (Points.Contains(new System.Windows.Point(y, x)))
                    { // Wanneer hij in een vakje is:
                        // TODO vorige kleur
                        HoveredRectangle.Fill = System.Windows.Media.Brushes.White;
                        HoveredRectangle = RectangleDictionary[new System.Windows.Point(y, x)];
                        RectangleDictionary[new System.Windows.Point(y, x)].Fill = System.Windows.Media.Brushes.Bisque;
                        RectangleDictionary[new System.Windows.Point(y, x)].Opacity = 25;

                    }
                }
            }*/

        }
        
        public void MouseClick(object sender, MouseButtonEventArgs e)
        {
            int y = Convert.ToInt32(e.GetPosition(Editor).Y - (e.GetPosition(Editor).Y % 25));
            int x = Convert.ToInt32(e.GetPosition(Editor).X - (e.GetPosition(Editor).X % 25));
            //int x = Convert.ToInt32(e.GetPosition(Editor).X);

            // controleer of de linkermuisknop ingedrukt werdt
            if (e.LeftButton == MouseButtonState.Pressed)
            {


                // het momentele punt instellen
                var currentpoint = new System.Windows.Point(y, x);

                // als er een vorig item geselcteerd is
                if (!LastSelected.Y.Equals(1000000))
                {
                    // het vorig geselecteerde punt instellen
                    var lastselected = RectangleDictionary[LastSelected];
                    
                    // als het punt al eerder geselecteerd is
                    if (LastSelected.Equals(currentpoint) || SelectedPoints.Contains(currentpoint))
                    {
                        // maakt hokje weer wit
                        RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.White;
                        // verwijderd hokje uit de lijst
                        SelectedPoints.Remove(currentpoint);
                    }
                    else // als het punt nog niet eerder geselecteerd is
                    {


                        // maakt alle tussenhokje magenta maar voegt ze niet toe aan de lijst

                        // kijk of de coord gelijk zijn met de x of de y
                        if (LastSelected.X == currentpoint.X)
                        {

                            // als het getal negatief is moet er naar boven worden getekend, positief is onder
                            if ((LastSelected.X - currentpoint.X < 0))
                            {
                                for (int i = (int)LastSelected.Y; i < currentpoint.Y; i -= 25)
                                {
                                    LastSelected.Y = i;
                                    RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.DarkMagenta;
                                }
                            }
                            else
                            {
                                for (int i = (int)LastSelected.Y; i < currentpoint.Y; i += 25)
                                {
                                    LastSelected.Y = i;
                                    RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.DarkMagenta;
                                }
                            }

                            // voegt hoekje toe aan lijst
                            SelectedPoints.Add(currentpoint);
                            // maakt hokje magenta
                            RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.DarkMagenta;
                            // maakt dit het laatste geselecteerde punt
                            LastSelected = currentpoint;
                        } else if (LastSelected.Y == currentpoint.Y)
                        {
                            // als het getal negatief is moet er naar links worden getekend, positief is rechts
                            if ((LastSelected.Y - currentpoint.Y < 0))
                            {
                                // zolang het vorige coordinaat kleiner is
                                for (int i = (int)LastSelected.X; i > currentpoint.X; i -= 25)
                                {
                                    LastSelected.X = i;
                                    RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.DarkMagenta;
                                }
                            }
                            else
                            {
                                for (int i = (int)LastSelected.X; i < currentpoint.X; i += 25)
                                {
                                    LastSelected.X = i;
                                    RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.DarkMagenta;
                                }
                            }
                            

                            // voegt hoekje toe aan lijst
                            SelectedPoints.Add(currentpoint);
                            // maakt hokje magenta
                            RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.DarkMagenta;
                            // maakt dit het laatste geselecteerde punt
                            LastSelected = currentpoint;
                        }// als het niet gelijk is foei
                        else
                        {
                            GeneralPopup warning = new GeneralPopup("sorry, je kunt niet schuin neerzetten");
                            warning.Show();
                        }


                    }
                    
                }
                else
                {
                    // kleurt het hokje in
                    RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.DarkMagenta;
                    // maakt dit het laatste geselecteerde punt
                    LastSelected = currentpoint;
                    // voegt het hokje toe aan de lijst
                    SelectedPoints.Add(currentpoint);

                }




            }

        }

    }
}

