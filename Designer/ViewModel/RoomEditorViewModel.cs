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
//using System.Threading;
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
        public List<Position> Last3HoveredPoints = new List<Position>(3);
        public Dictionary<Position, System.Windows.Shapes.Rectangle> RectangleDictionary = new Dictionary<Position, System.Windows.Shapes.Rectangle>();
        public Canvas Editor { get; set; }
        public Position LastSelected { get; set; } = new Position(-1, -1);

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
            var smallestX = SelectedPoints.Aggregate((p1, p2) => p1.X < p2.X ? p1 : p2);
            var smallestY = SelectedPoints.Aggregate((p1, p2) => p1.Y < p2.Y ? p1 : p2);
            List<Position> OffsetPositions = new List<Position>();
            foreach (var position in SelectedPoints)
            {
                OffsetPositions.Add(new Position(position.X - smallestX.X, position.Y - smallestY.Y));
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
           int y = Convert.ToInt32(e.GetPosition(Editor).Y - (e.GetPosition(Editor).Y % 25));
            int x = Convert.ToInt32(e.GetPosition(Editor).X - (e.GetPosition(Editor).X % 25));
            //int x = Convert.ToInt32(e.GetPosition(Editor).X);
            // stel punt in dat op dit moment wordt behoverd
            var currentpoint = new Position(x, y);


            Action UnHover = () =>
            {
                foreach (Position pos in Last3HoveredPoints)
                {
                    if (!SelectedPoints.Contains(pos) && !BorderPoints.Contains(pos))
                    // als als hij niet geselecteerd is en ook geen border is
                    {
                        RectangleDictionary[pos].Fill = System.Windows.Media.Brushes.White;
                        RectangleDictionary[pos].Opacity = 1;
                    }
                }
            };

            Action Bisqueinator = () =>
            {
                RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.Bisque;
                RectangleDictionary[currentpoint].Opacity = 0.5;
            };

            if (Last3HoveredPoints.Count > 0) 
                // als er al iets is gehovered
            {
                #region last3hoveredpoints
                if (Last3HoveredPoints.Count() == 3) 
                    // als de lijst al vol is
                {
                    // verwijder de eerste van de 3
                    Last3HoveredPoints.Remove(Last3HoveredPoints.First());
                }
                // voeg nieuwe aan lijst van gehoverede punten toe
                Last3HoveredPoints.Add(currentpoint);
                #endregion

                if (Last3HoveredPoints.Last().Equals(currentpoint) && RectangleDictionary.ContainsKey(currentpoint))
                    // als het item niet hetzelfde is al net behovered en als het hokje bestaat
                {
                    if (!SelectedPoints.Contains(currentpoint) && !BorderPoints.Contains(currentpoint))
                        // als als hij niet geselecteerd is en ook geen border is
                    {
                       
                        //Thread.Sleep(23000);
                        UnHover();
                        //kleuren
                        Bisqueinator();
                    }
                    // als het een border of hoek is
                    else
                    {
                        UnHover();
                        Last3HoveredPoints.Add(currentpoint);
                    }

                    }

            }
            else 
            // als er nog niks is gehovered
            {
                // voeg toe aan vorige 3 punten
                Last3HoveredPoints.Add(currentpoint);

                //kleuren
                Bisqueinator();
            }

            /* 
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
                //RectangleDictionary[LastHoveredRectangle].Fill = System.Windows.Media.Brushes.White;
                LastHoveredRectangle = currentpoint;
                RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.Bisque;
                RectangleDictionary[currentpoint].Opacity = 0.5;
            }
*/
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
                Action AddPoint = () =>
                {
                    // het geselecteerde punt kleuren en toevoegen aan de lijst van tussenpunten
                    RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.DarkMagenta;
                    RectangleDictionary[LastSelected].Opacity = 0.5;
                    BorderPoints.Add(LastSelected);
                };
                Action RemovePoint = () =>
                {
                    // het geselecteerde punt wit kleuren en toevoegen aan de lijst van tussenpunten
                    RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.White;
                    RectangleDictionary[LastSelected].Opacity = 1;
                    BorderPoints.Remove(LastSelected);
                };

                // als er geen vorig item geselcteerd is
                if (LastSelected.Y.Equals(-1))
                {
                    // kleurt het hokje in
                    RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.DarkMagenta;
                    // maakt dit het laatste geselecteerde punt
                    LastSelected = currentpoint;
                    // voegt het hokje toe aan de lijst
                    SelectedPoints.Add(currentpoint);
                    return;
                }

                var previouslySelected = LastSelected.Equals(currentpoint) /*|| SelectedPoints.Contains(currentpoint)*/;
                if (previouslySelected)
                {
                    RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.White;
                    SelectedPoints.Remove(currentpoint);
                    if (SelectedPoints.Count < 1)
                    {
                        LastSelected = new Position(-1, -1);
                        return;
                    }
                    else
                    {
                        LastSelected = SelectedPoints.Last();
                    }
                }

            if (!previouslySelected && (LastSelected.X != currentpoint.X && LastSelected.Y != currentpoint.Y))
                {
                    GeneralPopup warning = new GeneralPopup("sorry, je kunt niet schuin neerzetten");
                    warning.Show();
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
                        RectangleDictionary[currentpoint].Fill = System.Windows.Media.Brushes.DarkMagenta;
                        // maakt dit het laatste geselecteerde punt
                        
                    }
                    else
                    {
                        // vult vorige hokje weer in
                        LastSelected = SelectedPoints.Last();
                        RectangleDictionary[LastSelected].Fill = System.Windows.Media.Brushes.DarkMagenta;
                    }

                }

            }

        }
    }
}


