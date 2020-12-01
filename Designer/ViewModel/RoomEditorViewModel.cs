using Designer.Other;
using Designer.View;
using Models;
using Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
        public List<Position> Points = new List<Position>();
        public List<Position> SelectedPoints = new List<Position>();
        public List<Position> BorderPoints = new List<Position>();
        public Dictionary<Position, System.Windows.Shapes.Rectangle> RectangleDictionary = new Dictionary<Position, System.Windows.Shapes.Rectangle>();
        public Canvas Editor { get; set; }
        public Position LastSelected { get; set; } = new Position(-1, -1);
        public Position LastHoveredRectangle { get; set; }

        public Border CanvasBorder { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public ArgumentCommand<MouseEventArgs> MouseOverCommand { get; set; }
        public ArgumentCommand<MouseButtonEventArgs> MouseDownCommand { get; set; }
        public BasicCommand Submit { get; set; }
        private double CanvasHeight = 500;
        private double CanvasWidth = 1280;

        public RoomEditorViewModel()
        {
            Submit = new BasicCommand(SubmitRoom);
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
            var smallestposition =  SelectedPoints.Aggregate((p1, p2) => p1.X < p2.X || p1.Y < p2.Y ? p1 : p2);
           List<Position> OffsetPositions = new List<Position>();
            foreach (var position in SelectedPoints)
            {
                OffsetPositions.Add(new Position(position.X - smallestposition.X , position.Y- smallestposition.Y));
            }
            Room room = new Room(Name, Room.FromList(OffsetPositions));
            if (RoomService.Instance.Save(room) != null)
            {
                //opent successvol dialoog
                GeneralPopup popup = new GeneralPopup("De kamer is opgeslagen!");
                popup.ShowDialog();
                return;
            }
            //opent onsuccesvol dialoog
            GeneralPopup popuperror = new GeneralPopup("Er is iets misgegaan! probeer opnieuw.");
            popuperror.ShowDialog();
        }

        public void DrawGrid()
        {

            var y = 25 * 25;
            var x = 50 * 25;
            for (int row = 0; row < y; row += 25)
            {

                for (int column = 0; column < x; column += 25)
                {
                    System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                    rectangle.Fill = System.Windows.Media.Brushes.White;
                    rectangle.Width = 25;
                    rectangle.Height = 25;
                    rectangle.Stroke = System.Windows.Media.Brushes.Black;
                    Canvas.SetTop(rectangle, row);
                    Canvas.SetLeft(rectangle, column);
                    Position Point = new Position(column, row);
                    Points.Add(Point);
                    Editor.Children.Add(rectangle);
                    RectangleDictionary.Add(Point, rectangle);
                }
            }
        }
        public void MouseMove(object sender, MouseEventArgs e)
        {
          /*  int y = Convert.ToInt32(e.GetPosition(Editor).Y - (e.GetPosition(Editor).Y % 25));
            int x = Convert.ToInt32(e.GetPosition(Editor).X - (e.GetPosition(Editor).X % 25));
            //int x = Convert.ToInt32(e.GetPosition(Editor).X);

            var currentpoint = new Position(x, y);

            // als hiervoor al over een hokje gehovered is
            if (LastHoveredRectangle != null)
            {
                // als je niet nogsteeds over hetzelfde hokje hovered en als het hokje in de dictionary zit
                if (LastHoveredRectangle != currentpoint && RectangleDictionary.ContainsKey(currentpoint))
                {
                    // als het momenteel geselecteerde point niet een hoek of border is
                    if (!SelectedPoints.Contains(currentpoint) && !BorderPoints.Contains(currentpoint))
                    {
                        // vorige hokje terugkleuren als het geen border was
                        if (SelectedPoints.Contains(LastHoveredRectangle) && BorderPoints.Contains(LastHoveredRectangle))
                        {

                           
                        }
                        else
                        {
                            RectangleDictionary[LastHoveredRectangle].Fill = System.Windows.Media.Brushes.White;
                            RectangleDictionary[LastHoveredRectangle].Opacity = 1;
                            
                        }

                        // dit hokje inkleuren
                        RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.Bisque;
                        RectangleDictionary[currentpoint].Opacity = 0.5;

                        // stel vorige hokje in als deze
                        LastHoveredRectangle = currentpoint;
                    }
                    else
                    {
                        if (!SelectedPoints.Contains(LastHoveredRectangle) && !BorderPoints.Contains(LastHoveredRectangle))
                        {
                            RectangleDictionary[LastHoveredRectangle].Fill = System.Windows.Media.Brushes.White;
                            RectangleDictionary[LastHoveredRectangle].Opacity = 1;
                        }

                        // stel vorige hokje in als deze
                        LastHoveredRectangle = currentpoint;
                    }
                    // Wanneer hij in een vakje is:
                    // TODO vorige kleur

                }
            }
            else
            {
                RectangleDictionary[LastHoveredRectangle].Fill = System.Windows.Media.Brushes.White;
                LastHoveredRectangle = currentpoint;
                RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.Bisque;
                RectangleDictionary[currentpoint].Opacity = 0.5;
            }
*/
        }

        public void BorderRenderX(Position LastSelected, Position CurrentSelected, String Direction)
        {





        }
        public void BorderRenderY(Position LastSelected, Position CurrentSelected, String Direction)
        {





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
                var currentpoint = new Position(x, y);

                // als er een vorig item geselcteerd is
                if (!LastSelected.Y.Equals(-1))
                {
                    // het vorig geselecteerde punt instellen
                    //var lastselected = RectangleDictionary[LastSelected];

                    // als het punt al eerder geselecteerd is
                    if (LastSelected.Equals(currentpoint) || SelectedPoints.Contains(currentpoint))
                    {
                        //TODO tenzij het het eerste hokje is en selectedpoints groter is dan 3
                        // maakt hokje weer wit
                        RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.White;
                        // verwijderd hokje uit de lijst
                        SelectedPoints.Remove(currentpoint);
                    }
                    else // als het punt nog niet eerder geselecteerd is
                    {



                        // kijk of de coord gelijk zijn met de x of de y
                        if (LastSelected.X == currentpoint.X)
                        {

                            // als het getal negatief is moet er naar boven worden getekend, positief is onder
                            if (((LastSelected.Y - currentpoint.Y) >= 0))
                            {
                                for (int i = (int)LastSelected.Y; i != currentpoint.Y; i -= 25)
                                {
                                    LastSelected = new Position(LastSelected.X, i);
                                    // maakt alle tussenhokje magenta maar voegt ze niet toe aan de lijst
                                    RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.DarkMagenta;
                                    RectangleDictionary[LastSelected].Opacity = 0.5;
                                    BorderPoints.Add(LastSelected);

                                }

                            }
                            else
                            {
                                for (int i = (int)LastSelected.Y; i != currentpoint.Y; i += 25)
                                {
                                    LastSelected = new Position(LastSelected.X, i);
                                    RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.DarkMagenta;
                                    RectangleDictionary[LastSelected].Opacity = 0.5;
                                    BorderPoints.Add(LastSelected);

                                }

                            }

                            // voegt hoekje toe aan lijst
                            SelectedPoints.Add(currentpoint);
                            // maakt hokje magenta
                            RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.DarkMagenta;
                            // maakt dit het laatste geselecteerde punt
                            LastSelected = currentpoint;
                        }
                        else if (LastSelected.Y == currentpoint.Y)
                        {
                            // als het getal negatief is moet er naar links worden getekend, positief is rechts
                            if (((LastSelected.X - currentpoint.X) >= 0))
                            {
                                // zolang het vorige coordinaat kleiner is
                                for (int i = (int)LastSelected.X; i != currentpoint.X; i -= 25)
                                {
                                    LastSelected = new Position( i, LastSelected.Y);
                                    RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.DarkMagenta;
                                    RectangleDictionary[LastSelected].Opacity = 0.5;
                                    BorderPoints.Add(LastSelected);
                                }

                            }
                            else
                            {
                                for (int i = (int)LastSelected.X; i != currentpoint.X; i += 25)
                                {
                                    LastSelected = new Position(i, LastSelected.Y);
                                    RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.DarkMagenta;
                                    RectangleDictionary[LastSelected].Opacity = 0.5;
                                    BorderPoints.Add(LastSelected);
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

