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
//using System.Threading;
using System.Windows;
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
        public List<Line> GridLines { get; set; }
        public List<Position> Points = new List<Position>();
        public List<Position> SelectedPoints = new List<Position>();
        public List<Position> BorderPoints = new List<Position>();
        public Dictionary<Position, Rectangle> RectangleDictionary { get; set; }
        public List<Position> Last3HoveredPoints = new List<Position>(3);
        public Room SelectedRoom = new Room();
        public Canvas Editor { get; set; }
        public Position LastSelected { get; set; } = new Position(-1, -1);

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
        private Position _selectedPosition { get; set; }
        private Frame _activeFrame { get; set; }
        public List<Frame> FramePoints = new List<Frame>();
        private int _angle = 0;
        private double CanvasHeight = 500;
        private double CanvasWidth = 1280;

        // Constructor specially for unit testing
        public RoomEditorViewModel(string name) 
        {
            Name = name;
        }

        public RoomEditorViewModel()
        {
            GridLines = new List<Line>();
            RectangleDictionary = new Dictionary<Position, Rectangle>();
            Submit = new BasicCommand(SubmitRoom);
            MouseOverCommand = new ArgumentCommand<MouseEventArgs>(e => MouseMove(e.OriginalSource, e));
            MouseDownCommand = new ArgumentCommand<MouseButtonEventArgs>(e => MouseClick(e.OriginalSource, e));
            AddDoors = new BasicCommand(AddDoorsClick);
            AddWindows = new BasicCommand(AddWindowsClick);
            Editor = new Canvas();
            Reload();
        }

        public List<Position> MakeRoom(Room selectedroom)
        {
            SelectedPoints = Room.ToList(selectedroom.Positions);
            Position Last = new Position(-1, -1);

            foreach (Position pos in SelectedPoints)
            {
                if (!Last.Equals(new Position(-1, -1)))
                {
                    if (Last.Y == pos.Y)
                    {
                        // als het getal negatief is moet er naar links worden getekend, positief is rechts
                        var toRight = Last.X - pos.X >= 0;
                        // zolang het vorige coordinaat kleiner is
                        int i = (int)Last.X;
                        while (i != pos.X)
                        {
                            Last = new Position(i, Last.Y);
                            BorderPoints.Add(Last);
                            if (toRight)
                                i -= 25;
                            else
                                i += 25;
                        }
                    }
                    else
                    {
                        var toBottom = Last.Y - pos.Y >= 0;
                        int i = (int)Last.Y;

                        // zolang het vorige coordinaat kleiner is
                        while (i != pos.Y)
                        {
                            Last = new Position(Last.X, i);
                            BorderPoints.Add(Last);
                            if (toBottom)
                                i -= 25;
                            else
                                i += 25;
                        }
                    }
                    Last = new Position(pos.X, pos.Y);
                }
                else
                {
                    Last = new Position(pos.X, pos.Y);
                }

            }
            LastSelected = SelectedPoints.Last();
            return SelectedPoints;
        }

        public void PaintRoom()
        {
            foreach (Position pos in Points)
            {
                if (!BorderPoints.Contains(pos) && !SelectedPoints.Contains(pos))
                {
                    RectangleDictionary[pos].Fill = System.Windows.Media.Brushes.White;
                    RectangleDictionary[pos].Opacity = 1;
                }
            }

            foreach (Position pos in BorderPoints)
            {
                if(FramePoints.Exists(p => p.X == pos.X && p.Y == pos.Y))
                {
                    Frame frameFound = FramePoints.Where(p => p.X == pos.X && p.Y == pos.Y).First();
                    if(frameFound.Type == FrameTypes.Door)
                    {
                        RectangleDictionary[pos].Fill = System.Windows.Media.Brushes.Brown;
                        if(RectangleDictionary.ContainsKey(frameFound.AttachedPosition))
                        {
                            RectangleDictionary[frameFound.AttachedPosition].Fill = Brushes.White;
                        }
                    }
                    else if (frameFound.Type == FrameTypes.Window)
                    {
                        RectangleDictionary[pos].Fill = System.Windows.Media.Brushes.DarkBlue;
                    }

                    RectangleDictionary[pos].Opacity = 1;

                } else
                {
                    RectangleDictionary[pos].Fill = System.Windows.Media.Brushes.DarkMagenta;
                    RectangleDictionary[pos].Opacity = 0.5;
                }
            }
            foreach (Position pos in SelectedPoints)
            {
                RectangleDictionary[pos].Fill = System.Windows.Media.Brushes.DarkMagenta;
                RectangleDictionary[pos].Opacity = 1;
            }

            RectangleDictionary[SelectedPoints.Last()].Fill = System.Windows.Media.Brushes.Bisque;
        }

        public void SetSelectedRoom(Room selectedroom)
        {
            if (MakeRoom(selectedroom) != null)
            {
                PaintRoom();
            }
            else
            {
                GeneralPopup warning = new GeneralPopup("Oeps, er is iets mis gegaan!");
                warning.ShowDialog();
            }
            
        }
        public void Reload()
        { // Reload de items zodat de juiste te zien zijn
            Editor.Children.Clear();
            DrawGrid();
            RenderFrames();
            OnPropertyChanged();
        }
        private void OnPropertyChanged(string propertyName = "")
        {
            // herlaad de hele pagina
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void SubmitRoom()
        {
            if (SelectedPoints.Count() < 3)
            {
                GeneralPopup counterror = new GeneralPopup("Voer aub meer dan 2 punten in!.");
                counterror.ShowDialog();
                return;
            }
            // bepaal de kleinste x waarde
            var smallestX = SelectedPoints.Aggregate((p1, p2) => p1.X < p2.X ? p1 : p2);
            // bepaal de kleinste y waarde
            var smallestY = SelectedPoints.Aggregate((p1, p2) => p1.Y < p2.Y ? p1 : p2);
            // maakt de kamer zo hoog en links mogelijk
            List<Position> OffsetPositions = new List<Position>();
            foreach (var position in SelectedPoints)
            {
                OffsetPositions.Add(new Position(position.X - smallestX.X, position.Y - smallestY.Y));
            }

            List<RoomPlacement> framePositions = new List<RoomPlacement>();
            foreach (Frame frame in FramePoints)
            {
                framePositions.Add(new RoomPlacement(RoomPlacement.FromDimensions(frame.X - smallestX.X, frame.Y - smallestY.Y, frame.Type, frame.Rotation), frame.Rotation, frame.Type));
            }

            Room room = new Room(Name, Room.FromList(OffsetPositions), framePositions);

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
                    rectangle.StrokeThickness = 0.1;
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
            var mousePosition = e.GetPosition(Editor);

            int y = (int)(mousePosition.Y - (mousePosition.Y % 25));
            int x = (int)(mousePosition.X - (mousePosition.X % 25));

            Position point = new Position(x, y);

            // Sets hovered item (simplistic version of a hover)
            if(_selectedPosition != null && RectangleDictionary.ContainsKey(_selectedPosition))
            {
                RectangleDictionary[_selectedPosition].Fill = Brushes.White;
            }

            if(RectangleDictionary.ContainsKey(point) && RectangleDictionary[point].Fill == Brushes.White)
            {
                _selectedPosition = point;
                RectangleDictionary[point].Fill = Brushes.Bisque;
            }


            // Als de vorige positie is gezet wordt dit vervangen door de standaard kleur
            if (_previousCanvasPosition != null && !FramePoints.Exists(p => p.X == _previousCanvasPosition.X && p.Y == _previousCanvasPosition.Y))
            {
                RectangleDictionary[_previousCanvasPosition].Fill = Brushes.DarkMagenta;
                RectangleDictionary[_previousCanvasPosition].Opacity = 0.5;

                if (_activeFrame != null && _activeFrame.AttachedPosition != null && RectangleDictionary.ContainsKey(_activeFrame.AttachedPosition))
                {
                    RectangleDictionary[_activeFrame.AttachedPosition].Fill = Brushes.White;

                    int nextRotation = _activeFrame.Rotation + (_activeFrame.Rotation >= 180 ? -180 : 180);
                    Position oppositeSide = CalculateNextPositionFromAngle(nextRotation, _previousCanvasPosition.X, _previousCanvasPosition.Y);

                    if (RectangleDictionary.ContainsKey(oppositeSide))
                    {
                        RectangleDictionary[oppositeSide].Fill = Brushes.White;
                    }

                    _activeFrame.AttachedPosition = null;
                }
                _previousCanvasPosition = null;
            }

            if (AddDoorsChecked || AddWindowsChecked)
            {
                if (WithinSelectedPoints(x, y))
                {
                    // Kopieert de point naar een variable
                    _previousCanvasPosition = point;

                    // Berekend de horizontale posities links en rechts van het geplaatste object
                    Position horiLeft = new Position(x - 25, y);
                    Position horiRight = new Position(x + 25, y);

                    // Als het object locaties naast zich heeft wordt de rotatie naar 0 gezet en anders 90
                    int rotation = (RectangleDictionary.ContainsKey(horiLeft) && RectangleDictionary[horiLeft].Fill != Brushes.White) || (RectangleDictionary.ContainsKey(horiRight) && RectangleDictionary[horiRight].Fill != Brushes.White) ? 0 : 90;

                    // Haalt de positie uit de lijst als die daar bestaat
                    if (AddWindowsChecked)
                    {
                        Frame window = new Frame(point.X, point.Y, FrameTypes.Window);

                        _activeFrame = window;

                        if (FramePoints.Contains(window)) return;

                        window.Rotation = rotation;

                        RectangleDictionary[point].Fill = Brushes.DarkBlue;
                        RectangleDictionary[point].Opacity = 1.0;
                    }

                    if (AddDoorsChecked)
                    {
                        Frame door = new Frame(point.X, point.Y, FrameTypes.Door);

                        _activeFrame = door;

                        if (FramePoints.Contains(door)) return;

                        // Als de focus van horizontaal naar verticaal gaat moet de rotatie veranderen, anders mag de opgeslagen rotatie gebruikt worden. Dit is ook vice versa
                        door.Rotation = _angle == rotation || _angle == (rotation + 180) ? _angle : rotation;

                        RectangleDictionary[point].Fill = Brushes.Brown;
                        RectangleDictionary[point].Opacity = 1.0;

                        RotateDoor(door.Rotation, x, y);

                        //Position attachedPosition = new Position(x, y);

                        //// Haalt de framepoint op uit de framepoint selectie, vervolgens wordt er gekeken of het actieve punt wel bestaat in de framepoints
                        //Frame frameInList = FramePoints.Where(p => p == attachedPosition).FirstOrDefault();

                        //if (frameInList != null)
                        //{
                        //    RectangleDictionary[frameInList.AttachedPosition].Fill = Brushes.White;
                        //}
                    }
                }
            }
        }

        public void RenderFrames()
        {
            if(FramePoints != null)
            {
                foreach (Frame frame in FramePoints)
                {
                    if(frame.Type == FrameTypes.Door)
                    {
                        Position doorPosition = new Position(frame.X, frame.Y);
                        Position doorOpenPosition = CalculateNextPositionFromAngle(frame.Rotation, frame.X, frame.Y);

                        RectangleDictionary[doorPosition].Fill = Brushes.Brown;
                        RectangleDictionary[doorPosition].Opacity = 1.0;

                        if(RectangleDictionary.ContainsKey(doorOpenPosition))
                        {
                            RectangleDictionary[doorOpenPosition].Fill = Brushes.Brown;
                            RectangleDictionary[doorOpenPosition].Opacity = 1.0;
                        }
                    }

                    if(frame.Type == FrameTypes.Window)
                    {
                        Position windowPosition = new Position(frame.X, frame.Y);

                        RectangleDictionary[windowPosition].Fill = Brushes.DarkBlue;
                        RectangleDictionary[windowPosition].Opacity = 1.0;
                    }
                }
            }
        }

        public void RotateDoor(int angle, int x, int y)
        {
            if(_activeFrame != null)
            {
                //if(_activeFrame.AttachedPosition != null && RectangleDictionary.ContainsKey(_activeFrame.AttachedPosition))
                //{
                //    RectangleDictionary[_activeFrame.AttachedPosition].Fill = Brushes.White;
                //}

                Position doorOpenPos = CalculateNextPositionFromAngle(angle, x, y);

                if (!FramePoints.Contains(_activeFrame))
                {
                    _activeFrame.AttachedPosition = doorOpenPos;

                    // Als de positie niet in de editor bestaat wordt er niks ingekleurd
                    if(RectangleDictionary.ContainsKey(doorOpenPos))
                    {
                        RectangleDictionary[doorOpenPos].Fill = Brushes.Brown;
                    }
                }
            }
        }

        public Position CalculateNextPositionFromAngle(int angle, int x, int y)
        {
            switch (angle)
            {
                case 0:
                    y -= 25;
                    break;
                case 90:
                    x += 25;
                    break;
                case 180:
                    y += 25;
                    break;
                case 270:
                    x -= 25;
                    break;
            }

            return new Position(x, y);
        }
        public void MouseClick(object sender, MouseButtonEventArgs e)
        {
            int y = Convert.ToInt32(e.GetPosition(Editor).Y - (e.GetPosition(Editor).Y % 25));
            int x = Convert.ToInt32(e.GetPosition(Editor).X - (e.GetPosition(Editor).X % 25));

            Position currentpoint = new Position(x, y);

            if (e.RightButton == MouseButtonState.Pressed && _activeFrame != null)
            {
                if(_activeFrame != null && _activeFrame.Type == FrameTypes.Door)
                {
                    int nextRotation = _activeFrame.Rotation + (_activeFrame.Rotation >= 180 ? -180 : 180);
                    _activeFrame.Rotation = nextRotation;
                    _angle = nextRotation;
                    RotateDoor(_activeFrame.Rotation, x, y);
                }
            }

            // controleer of de linkermuisknop ingedrukt werdt
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if(WithinSelectedPoints(x, y) && _activeFrame != null)
                {
                    if (AddWindowsChecked)
                    {
                        _activeFrame.Type = FrameTypes.Window;
                    } else if(AddDoorsChecked)
                    {
                        _activeFrame.Type = FrameTypes.Door;
                    }

                    if (FramePoints.Contains(_activeFrame))
                    {
                        FramePoints.Remove(_activeFrame);
                    }
                    else
                    {
                        FramePoints.Add(_activeFrame);
                    }
                    _activeFrame = null;
                    return;
                }

                // het momentele punt instellen
                Action AddPoint = () =>
                {
                    // het geselecteerde punt kleuren en toevoegen aan de lijst van tussenpunten
                    /*RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.DarkMagenta;
                    RectangleDictionary[LastSelected].Opacity = 0.5;*/
                    BorderPoints.Add(LastSelected);
                };
                Action RemovePoint = () =>
                {
                    // het geselecteerde punt wit kleuren en toevoegen aan de lijst van tussenpunten
                    /*RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.White;
                    RectangleDictionary[LastSelected].Opacity = 1;*/
                    BorderPoints.Remove(LastSelected);
                };

                // als er geen vorig item geselcteerd is
                if (LastSelected.Y.Equals(-1))
                {
                    // kleurt het hokje in
                   // RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.DarkMagenta;
                    // maakt dit het laatste geselecteerde punt
                    LastSelected = currentpoint;
                    // voegt het hokje toe aan de lijst
                    SelectedPoints.Add(currentpoint);
                    return;
                }

                var previouslySelected = LastSelected.Equals(currentpoint) /*|| SelectedPoints.Contains(currentpoint)*/;
                if (previouslySelected)
                {  // Als de laatst geselecteerde rectangle hetzelfde is als currentpoint
                    //RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.White; // Maak deze rectactangle wit
                    SelectedPoints.Remove(currentpoint); // En verwijder hem van SelectedPoints
                    if (SelectedPoints.Count < 1)
                    { // Als het aantal selectedpoints kleiner dan 1 is
                        LastSelected = new Position(-1, -1);
                        return; // zet lastselected op een positie die niet bestaat
                    }
                    else
                    { // Anders is de lastselected de laatst geselecteerde
                        LastSelected = SelectedPoints.Last();
                    }
                }

            if (!previouslySelected && (LastSelected.X != currentpoint.X && LastSelected.Y != currentpoint.Y))
                {  // Als je hem schuin zet
                    GeneralPopup warning = new GeneralPopup("sorry, je kunt niet schuin neerzetten");
                    warning.Show(); // Warning met "sorry, je kunt niet schuin neerzetten"
                    return;
                }

                // verwijder borders
                if (SelectedPoints != null)
                {
                    if (LastSelected.Y == currentpoint.Y)
                    {
                        // als het getal negatief is moet er naar links worden getekend, positief is rechts
                        var toRight = LastSelected.X - currentpoint.X >= 0;
                        // zolang het vorige coordinaat kleiner is
                        int i = (int)LastSelected.X;
                        while (i != currentpoint.X)
                        {
                            LastSelected = new Position(i, LastSelected.Y);
                            if (previouslySelected)
                                RemovePoint();
                            else
                                AddPoint();
                            if (toRight)
                                i -= 25;
                            else
                                i += 25;
                        }
                    }
                    else
                    {
                        var toBottom = LastSelected.Y - currentpoint.Y >= 0;
                        int i = (int)LastSelected.Y;

                        // zolang het vorige coordinaat kleiner is
                        while (i != currentpoint.Y)
                        {
                            LastSelected = new Position(LastSelected.X, i);
                            if (previouslySelected)
                                RemovePoint();
                            else
                                AddPoint();
                            if (toBottom)
                                i -= 25;
                            else
                                i += 25;
                        }
                    }

                    if (!previouslySelected)
                    {
                        LastSelected = currentpoint;
                        // voegt hoekje toe aan lijst
                        SelectedPoints.Add(LastSelected);
                        // maakt hokje magenta
                        //RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.DarkMagenta;
                        // maakt dit het laatste geselecteerde punt
                        
                    }
                    else
                    {
                        // vult vorige hokje weer in
                        LastSelected = SelectedPoints.Last();
                        //RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.DarkMagenta;
                    }

                }

            }
            PaintRoom();
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

        public bool WithinSelectedPoints(int x, int y)
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

            return valid;
        }
    }

    public class Frame : Position
    {
        public FrameTypes Type { get; set; }
        public int Rotation { get; set; }

        public Position? AttachedPosition { get; set; }

        public Frame(int x, int y, FrameTypes type): base(x, y)
        {
            Type = type;
        }
    }
}


