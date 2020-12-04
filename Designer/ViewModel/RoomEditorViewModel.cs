using Designer.Other;
using Designer.View;
using Models;
using Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
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
        public BasicCommand AddDoors { get; set; }
        public BasicCommand AddWindows { get; set; }
        public bool AddDoorsChecked { get; set; } = false;
        public bool AddWindowsChecked { get; set; } = false;
        private Position _previousCanvasPosition { get; set; }
        private Frame _activeFrame { get; set; }
        public List<Frame> FramePoints = new List<Frame>();
        private double CanvasHeight = 500;
        private double CanvasWidth = 1280;

        public RoomEditorViewModel()
        {
            Submit = new BasicCommand(SubmitRoom);
            MouseOverCommand = new ArgumentCommand<MouseEventArgs>(e => MouseMove(e.OriginalSource, e));
            MouseDownCommand = new ArgumentCommand<MouseButtonEventArgs>(e => MouseClick(e.OriginalSource, e));
            AddDoors = new BasicCommand(AddDoorsClick);
            AddWindows = new BasicCommand(AddWindowsClick);
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
            var smallestposition =  SelectedPoints.Aggregate((p1, p2) => p1.X < p2.X || p1.Y < p2.Y ? p1 : p2); // Om te kijken welke van de punten de kleinste is
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

            var y = 25 * 25; // Scherm is 25 vakjes hoog
            var x = 50 * 25; // Scherm is 50 vakjes breed
            for (int row = 0; row < y; row += 25)
            { // Voor elke rij
                for (int column = 0; column < x; column += 25)
                { // In deze rij voor elke kolom
                    System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                    rectangle.Fill = System.Windows.Media.Brushes.White;
                    rectangle.Width = 25;
                    rectangle.Height = 25;
                    rectangle.Stroke = System.Windows.Media.Brushes.Black; // Maak vierkant
                    Canvas.SetTop(rectangle, row);
                    Canvas.SetLeft(rectangle, column);
                    Position Point = new Position(column, row); // Op de juiste plek
                    Points.Add(Point);
                    Editor.Children.Add(rectangle);
                    RectangleDictionary.Add(Point, rectangle); // Maak hem aan
                }
            }
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {

            Debug.WriteLine("testest");
            var mousePosition = e.GetPosition(Editor);

            int y = (int)(mousePosition.Y - (mousePosition.Y % 25));
            int x = (int)(mousePosition.X - (mousePosition.X % 25));

            Position point = new Position(x, y);

            Debug.WriteLine("Exists: " + FramePoints.Exists(p => p.X == point.X && p.Y == point.Y));

            // Als de vorige positie is gezet wordt dit vervangen door de standaard kleur
            if (!FramePoints.Exists(p => p.X == point.X && p.Y == point.Y) && _previousCanvasPosition != null)
            {
                RectangleDictionary[_previousCanvasPosition].Fill = Brushes.DarkMagenta;
                RectangleDictionary[_previousCanvasPosition].Opacity = 0.5;
                _previousCanvasPosition = null;
            }

            if (AddDoorsChecked || AddWindowsChecked)
            {
                List<Position> points = SelectedPoints;

                bool valid = false;

                for (int i = 0; i < (points.Count - 1); i++)
                {
                    int nextIncrement = i + 1;

                    int highX = points[i].X > points[nextIncrement].X ? points[i].X : points[nextIncrement].X;
                    int lowX = points[i].X > points[nextIncrement].X ? points[nextIncrement].X : points[i].X;

                    int highY = points[i].Y > points[nextIncrement].Y ? points[i].Y : points[nextIncrement].Y;
                    int lowY = points[i].Y > points[nextIncrement].Y ? points[nextIncrement].Y : points[i].Y;

                    if ((x > lowX && x < highX && y == points[i].Y) || (y > lowY && y < highY && x == points[i].X))
                    {
                        valid = true;
                        break;
                    }
                }

                if(valid)
                {
                    // Kopieert de point naar een variable
                    _previousCanvasPosition = point;

                    if (AddWindowsChecked)
                    {
                        Frame window = new Frame(point.X, point.Y, FrameTypes.Window);

                        RectangleDictionary[point].Fill = Brushes.DarkBlue;
                        RectangleDictionary[point].Opacity = 1.0;
                        _activeFrame = window;
                    }

                    if(AddDoorsChecked)
                    {
                        Frame door = new Frame(point.X, point.Y, FrameTypes.Door);

                        RectangleDictionary[point].Fill = Brushes.DarkBlue;

                        Position horiLeft = new Position(x - 25, y);
                        Position horiRight = new Position(x + 25, y);

                        door.Rotation = RectangleDictionary[horiLeft] != null && RectangleDictionary[horiRight] != null ? 0 : 90;

                        RectangleDictionary[point].Fill = Brushes.Brown;
                        RectangleDictionary[point].Opacity = 1.0;

                        RotateDoor(door.Rotation, x, y);
                    }
                }


                //for (int i = 0; i < points.Count; i++)
                //{
                //    Debug.WriteLine($"{points[i].X} - {points[i + 1].X}");
                //    if(i < (points.Count - 1) && points[i].X == points[i + 1].X)
                //    {
                //        Debug.WriteLine("Same X values");
                //    }
                //}
            }

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

        public void RotateDoor(int angle, int x, int y)
        {
            if(_activeFrame != null)
            {
                int doorOpenX = x;
                int doorOpenY = y;

                switch (angle)
                {
                    case 0:
                        doorOpenY -= 15;
                        break;
                    case 90:
                        doorOpenX += 25;
                        break;
                    case 180:
                        doorOpenY += 25;
                        break;
                    case 270:
                        doorOpenX -= 25;
                        break;
                }

                Position doorOpenPos = new Position(doorOpenX, doorOpenY);
            }
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

            if(e.RightButton == MouseButtonState.Pressed && _activeFrame != null)
            {
                Debug.WriteLine("Rotate door");
            }

            // controleer of de linkermuisknop ingedrukt werdt
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // het momentele punt instellen
                var currentpoint = new Position(x, y);

                if(AddWindowsChecked)
                {
                    Frame window = new Frame(x, y, FrameTypes.Window);
                    FramePoints.Add(window);
                }


                // als er een vorig item geselcteerd is
                else if (!LastSelected.Y.Equals(-1))
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

        public void AddDoorsClick()
        {
            if(AddDoorsChecked) AddWindowsChecked = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void AddWindowsClick()
        {
            if(AddWindowsChecked) AddDoorsChecked = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }
    }

    public enum FrameTypes
    {
        Door,
        Window
    }

    public class Frame : Position
    {
        public FrameTypes Type { get; set; }
        public int Rotation { get; set; }

        public Frame(int x, int y, FrameTypes type): base(x, y)
        {
            Type = type;
        }
    }
}

