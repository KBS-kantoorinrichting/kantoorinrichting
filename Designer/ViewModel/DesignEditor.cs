using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Designer.Other;
using Designer.View;
using Models;
using Models.Utils;
using Services;
using Line = System.Windows.Shapes.Line;
using Polygon = System.Windows.Shapes.Polygon;

namespace Designer.ViewModel
{
    public class DesignEditor : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<Product> Products { get; set; }
        private Dictionary<Product, ProductData> _productOverview { get; set; }
        public List<KeyValuePair<Product, ProductData>> ProductOverview => _productOverview.ToList();
        public double TotalPrice => _productOverview.Sum(p => p.Value.TotalPrice);
        public List<ProductPlacement> ProductPlacements { get; set; }
        public List<RoomPlacement> RoomPlacements { get; set; }
        public ArgumentCommand<DragEventArgs> DragDropCommand { get; set; }
        public ArgumentCommand<DragEventArgs> DragOverCommand { get; set; }
        public ArgumentCommand<MouseButtonEventArgs> CatalogusMouseDownCommand { get; set; }
        public ArgumentCommand<MouseButtonEventArgs> CanvasMouseDownCommand { get; set; }
        public ArgumentCommand<MouseEventArgs> MouseMoveCommand { get; set; }
        public BasicCommand Measure { get; set; }
        public BasicCommand GotoDesigns { get; set; }
        public BasicCommand Plexiglass { get; set; }
        public BasicCommand Layout { get; set; }
        public BasicCommand ClearProducts { get; set; }
        public BasicCommand GenerateRoute { get; set; }
        public BasicCommand GeneratePlexiline { get; set; }
        public BasicCommand RemoveRoute { get; set; }
        public BasicCommand Save { get; set; }
        public ArgumentCommand<MouseWheelEventArgs> CanvasMouseScrollCommand { get; set; }
        public Product SelectedProduct => _selectedPlacement.Product;
        public Design Design { get; set; }
        public Canvas Editor { get; set; }
        private Point _previousPosition;
        private ProductPlacement _selectedPlacement;
        private ProductPlacement _draggingPlacement;
        public Polygon RoomPoly { get; set; }
        public bool AllowDrop = false;

        public int DistanceScore
        {
            get
            {
                // maakt lijst van lijnen
                List<DistanceLine> distanceLines = _lines
                    .Where(e => e.Key != _fakeRoute)
                    .Select(e => e.Value)
                    .SelectMany(v => v)
                    .Where(e => e.Key != _fakeRoute)
                    .Select(e => e.Value)
                    .Distinct()
                    .ToList();

                // telt lijnen
                double count = distanceLines.Count(l => !l.Shows);
                int m = distanceLines.Count == 0 ? 100 : (int)(count / distanceLines.Count * 100);

                //                                                                             Groen       Rood
                DistanceColour = (SolidColorBrush) new BrushConverter().ConvertFrom(m > 99 ? "#00D092" : "#d00037");
                OnPropertyChanged("DistanceColour");
                
                return m;
            }
        }
        

        public int RouteScore {
            get {
                // maakt lijst van lijnen
                List<DistanceLine> distanceLines = _lines
                    .Where(e => e.Key == _fakeRoute)
                    .Select(e => e.Value)
                    .SelectMany(v => v.Values)
                    .Distinct()
                    .ToList();

                // telt lijnen
                double count = distanceLines.Count(l => !l.Shows);
                int m = distanceLines.Count == 0 ? 100 : (int)(count / distanceLines.Count * 100);

                //                                                                            Groen       Rood
                RouteColour = (SolidColorBrush) new BrushConverter().ConvertFrom(m == 100 ? "#00D092" : "#d00037");
                OnPropertyChanged("RouteColour");
                
                return m;
            }
        }
        
        public int VentilationScore
        {
            get
            {
                // als er niks is 0
                if (RoomPlacements == null || RoomPlacements.Count == 0) return 0;
                // minder dan 2 is ook een onvoldoende
                if (RoomPlacements.Count < 2) return 0;
                // maakt lijst van x en y coordinaten (+1) voor de calculatie (voorbereiding Pick's theorem)
                List<Position> roompositions = new List<Position>();
                roompositions = Room.ToList(Design.Room.Positions);
                List<int> xlist = new List<int>();
                roompositions.ForEach(l => xlist.Add((int)l.X));
                List<int> ylist = new List<int>();
                roompositions.ForEach(l => ylist.Add((int)l.Y));
                // 1x extra toevoegen omdat de laatste x de eerste moet
                xlist.Add(roompositions.First().X);
                ylist.Add(roompositions.First().Y);
                int i = 0;
                double cm2 = 0;
                // cm2 calculatie (Pick's theorem)
                foreach (Position pos in roompositions)
                {
                    cm2 += (((xlist[i] * ylist[i + 1]) - (ylist[i] * xlist[i + 1])) / 2);
                    i++;
                }
                // convert negatief getal naar positief
                if (cm2 < 0) { cm2 = cm2 * -1; }
                int m = (int)(cm2 / 10000);
                m = 100 - (m - RoomPlacements.Count);
                
                //                                                                                Groen       Rood
                VentilationColour = (SolidColorBrush) new BrushConverter().ConvertFrom(m > 55 ? "#00D092" : "#d00037");
                OnPropertyChanged("VentilationColour");
                
                return m;
            }
        }

        
        public Brush DistanceColour { get; set; }
        public Brush VentilationColour { get; set; }
        public Brush RouteColour { get; set; }

        public double Scale = 1.0;
        private double _canvasHeight => Navigator.Instance.CurrentPage.ActualHeight - 20;

        private double _canvasWidth => Navigator.Instance.CurrentPage.ActualWidth - 260;
        private readonly MatrixTransform _transform = new MatrixTransform();
        private Point _initialMousePosition;

        public List<Models.Polygon> PlexiLines = new List<Models.Polygon>();
        public List<DistanceLine> DistancePlexiLines = new List<DistanceLine>();

        public bool PlexiEnabled { get; set; }

        //private Position _pSecondPoint;
        private DistanceLine _plexiLine;

        //Special constructor for unit tests
        public DesignEditor(Design design)
        {
            SetDesign(design);
            Products = LoadProducts();
        }

        public DesignEditor()
        {
            GotoDesigns = new PageCommand(() => {
                DesignCatalog DesignCatalog = new DesignCatalog();
                DesignCatalog.DesignSelected += (o, e) =>
                {
                    Navigator.Instance.Replace(new ViewDesignPage( e.Value));
                };
                return DesignCatalog;
            });
            Products = LoadProducts();
            Editor = new Canvas();
           
            RoomPoly = new Polygon();
            CatalogusMouseDownCommand =
                new ArgumentCommand<MouseButtonEventArgs>(e => CatalogusMouseDown(e.OriginalSource, e));
            CanvasMouseDownCommand =
                new ArgumentCommand<MouseButtonEventArgs>(e => CanvasMouseDown(e.OriginalSource, e));
            DragDropCommand = new ArgumentCommand<DragEventArgs>(e => CanvasDragDrop(e.OriginalSource, e));
            DragOverCommand = new ArgumentCommand<DragEventArgs>(e => CanvasDragOver(e.OriginalSource, e));
            MouseMoveCommand = new ArgumentCommand<MouseEventArgs>(HandleMouseMove);
            Measure = new BasicCommand(StartMeasure);
            Plexiglass = new BasicCommand(StartPlexiglass);
            Layout = new BasicCommand(GenerateLayout);
            GenerateRoute = new BasicCommand(GenerateWalkRoute);
            GeneratePlexiline = new BasicCommand(GeneratePlexi);
            RemoveRoute = new BasicCommand(DeleteRoute);
            ClearProducts = new BasicCommand(Clear);
            Save = new BasicCommand(() => DesignService.Instance.SaveChanges());
            CanvasMouseScrollCommand =
                new ArgumentCommand<MouseWheelEventArgs>(e => CanvasMouseScroll(e.OriginalSource, e));
            _productOverview = new Dictionary<Product, ProductData>();

            _distanceLine = new DistanceLine(null, null);
            _plexiLine = new DistanceLine(null, null, "(Plexiglas)");
        }

        public bool Enabled { get; set; }

        private Position _origin;
        private Position _secondPoint;
        private DistanceLine _distanceLine;

        //Verwijder de lijn als er al een meetlat was
        public void StartMeasure()
        {
            if (!Enabled) return;
            _distanceLine.Remove(Editor);
            _origin = null;
            _secondPoint = null;
        }

        public void StartPlexiglass() {
            // initializeerd de waardes gebruikt voor het plaatsen van plexiglas
            _origin = null;
            _secondPoint = null;
        }

        public bool RouteEnabled { get; set; }

        private Models.Polygon _route
        {
            get => Design.GetRoutePoly();
            set => Design.Route = value?.Convert();
        }

        /**
         * Verwijderd de route
         */
        public void DeleteRoute()
        {
            _route = null;
            
            RemoveCorona(_fakeRoute);
            RenderRoute();
        }

        public void GeneratePlexi() {
            // genereerdt plexiglas
            foreach (DistanceLine distanceLine in _lines.Values
                .SelectMany(d => d.Values)
                .Distinct()) {
                Models.Line line = new Models.Line(distanceLine.P1, distanceLine.P2);
                line = line.RightAngleLine();
                if (line == null) continue;
                PlexiLines.Add(new Models.Polygon(line.AsList()));
            }
            UpdateDbPlexiglass();
            RenderPolyPlexi();
        }

        public void GenerateWalkRoute()
        {
            // genereer loop route
            int distance = 50;

            List<Models.Line> lines = Design.Room.GetPoly().GetLines().ToList();
            List<Models.Line> correct = lines.Select(l => (Models.Line)null).ToList();
            //Gaat door alle hoeken (lijn paren) heen om te kijken waar maar 1 mogelijk is, om hiervandaan te starten
            int start = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                Models.Line l1 = lines[i];
                Models.Line l2 = lines[(i + 1) % lines.Count];

                Models.Line foundL1 = null;
                Models.Line foundL2 = null;
                for (int r = 0; r < 4; r++)
                {
                    Models.Line tempL1 = l1.OffsetPerpendicular(distance, r % 2 == 0);
                    Models.Line tempL2 = l2.OffsetPerpendicular(distance, r / 2 == 0);

                    Position inter = tempL1.Intersection(tempL2);
                    if (inter == null || !Design.Room.GetPoly().Inside(inter)) continue;
                    //Als die een tweede punt vind dan is dit geen geldige hoek
                    if (foundL1 != null)
                    {
                        foundL1 = null;
                        foundL2 = null;
                        break;
                    }

                    foundL1 = tempL1;
                    foundL2 = tempL2;
                }

                if (foundL1 == null) continue;
                correct[i] = foundL1;
                start = (i + 1) % lines.Count;
                correct[start] = foundL2;
                break;
            }

            //Start bij de eerste hoek waar maar 1 mogelijk punt is en pakt vervolgens altijd de verste afstand hiervan voor de volgende lijn
            for (int i = start; i != start - 1; i = (i + 1) % lines.Count)
            {
                int j = (i + 1) % lines.Count;
                if (correct[j] != null) break;
                Models.Line before = correct[i];
                Models.Line toTest = lines[j];

                Models.Line l1 = toTest.OffsetPerpendicular(distance, true);
                Models.Line l2 = toTest.OffsetPerpendicular(distance, false);

                Position inter1 = before.Intersection(l1);
                if (inter1 == null || !Design.Room.GetPoly().Inside(inter1))
                {
                    correct[j] = l2;
                    continue;
                }

                Position inter2 = before.Intersection(l2);
                if (inter2 == null || !Design.Room.GetPoly().Inside(inter2))
                {
                    correct[j] = l1;
                    continue;
                }

                double d1 = before.P1.Distance(inter1);
                double d2 = before.P1.Distance(inter2);

                correct[j] = d1 > d2 ? l1 : l2;
            }

            //Zoekt voor alle lijn de snijpunten om de route te maken
            List<Position> positions = new List<Position>();
            for (int i = 0; i < correct.Count; i++)
            {
                Models.Line l1 = correct[i];
                Models.Line l2 = correct[(i + 1) % lines.Count];
                positions.Add(l1.Intersection(l2));
            }

            _route = new Models.Polygon(positions);
            RenderRoute();
        }

        List<DistanceLine> _routeLines = new List<DistanceLine>();
        List<Ellipse> _ellipses = new List<Ellipse>();

        /**
         * Tekend de volledige route
         */
        public void RenderRoute()
        {
            //Verwijderd eerst de volledige lijn
            _routeLines.ForEach(l => l.Remove(Editor));
            _routeLines.Clear();
            _ellipses.ForEach(Editor.Children.Remove);
            _ellipses.Clear();
            if (_route == null || _route.Count == 0) return;
            //Tekend de volledige lijn
            foreach (Models.Line line in _route.GetLines())
            {
                _routeLines.Add(new DistanceLine(line.P1, line.P2));
            }

            //Tekend alle hoek punten
            for (int index = 0; index < _route.Count; index++)
            {
                Position position = _route[index];
                int size = 10;
                Ellipse ellipse = new Ellipse()
                {
                    Height = size,
                    Width = size,
                    Fill = Brushes.Black,
                    Uid = index.ToString(),
                };

                _ellipses.Add(ellipse);
                Editor.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, position.X - size / 2);
                Canvas.SetTop(ellipse, position.Y - size / 2);
                Panel.SetZIndex(ellipse, 300);
            }

            foreach (RoomPlacement placement in Design.Room.RoomPlacements) {
                if (placement.Type != FrameTypes.Door) continue;
                (Position p1, Position p2) = placement.GetPoly().MinDistance(_route);
                _routeLines.Add(new DistanceLine(p1, p2));
            }

            _routeLines.ForEach(l => l.Add(Editor));

            //Controlleerd alle plaatsing tegen over de route
            _fakeRoute.poly = _route;
            CheckCorona(_fakeRoute);
        }

        /**
         * Verwijderd alle plaatsingen
         */
        public void Clear()
        {
            ProductPlacements.ForEach(RemoveCorona);
            ProductPlacements.Clear();
            OnPropertyChanged();
            RenderRoom();
        }

        /**
         * Plaatst alle deuren en ramen die in de ruimte zitten
         */
        public void RenderRoomFrames() {
            if (Design.Room.RoomPlacements != null) {
                foreach (RoomPlacement frame in Design.Room.RoomPlacements) {
                    Position pos = RoomPlacement.ToPosition(frame.Positions);
                    Polygon newPoly = new Polygon();

                    if (frame.Type == FrameTypes.Door) {
                        int x = (int) pos.X;
                        int y = (int) pos.Y;

                        if (frame.Rotation == 0) y -= 25;
                        if (frame.Rotation == 270) x -= 25;

                        PointCollection points = new PointCollection() {
                            new Point(x, y),
                            new Point(x + 25, y),
                            new Point(x + 25, y + 25),
                            new Point(x, y + 25)
                        };

                        newPoly.Points = points;
                        newPoly.Fill = Brushes.Brown;
                        Editor.Children.Add(newPoly);
                    }

                    if (frame.Type == FrameTypes.Window) {
                        List<Position> roomPositions = Room.ToList(Design.Room.Positions);
                        Debug.WriteLine(roomPositions);

                        Position startPosition = RoomPlacement.ToPosition(frame.Positions);
                        Position roomPosition =
                            roomPositions.FirstOrDefault(p => p.X == startPosition.X || p.Y == startPosition.Y);

                        bool vertical = startPosition.X == roomPosition.X;

                        Line window = new Line {
                            X1 = startPosition.X,
                            Y1 = startPosition.Y,
                            X2 = vertical ? startPosition.X : startPosition.X + 25,
                            Y2 = vertical ? startPosition.Y + 25 : startPosition.Y,
                            StrokeThickness = 8,
                            Stroke = Brushes.DarkBlue
                        };
                        Editor.Children.Add(window);
                    }
                }
            }
        }

        /**
         * Loopt alle mogelijke plaatsingen door om vervolgens de kamer zo vol mogelijk te krijgen
         */
        public void GenerateLayout()
        {
            Models.Polygon room = Design.Room.GetPoly();
            //TODO Pakt momententeel nog het eerste product om te plaatsen
            Product product = Products.First();

            //Pakt de minimale en maximale punten
            Position min = room.Min();
            Position max = room.Max();

            //Berekend de afstand per stap door middel van de afstand tussen de 2 boudning hoeken punten,
            //als dit te groot wordt pakt die de product lengte of hoogte ligt er aan welke kleiner is 
            int accuracy = Math.Min((int)min.Distance(max) / 200, Math.Min(product.Length, product.Width));

            //Maakt een nieuwe thread aan waar de dingen gecontrolleerd worden zodat je live de producten ziet plaatsen
            new Thread(
                () =>
                {
                    //Loopt door alle coordinaten binnen de ruimte boudning box heen met stappen van accuracy
                    for (int y = min.Y + 1; y < max.Y; y += accuracy)
                    {
                        for (int x = min.X + 1; x < max.X; x += accuracy)
                        {
                            Position position = new Position(x, y);

                            //Als het punt binnen de ruimte zit controlleerd die of deze genoeg afstand heeft van alles
                            if (room.Inside(product.GetPoly().Offset(position)))
                            {
                                ProductPlacement placement = new ProductPlacement(position, product, null);
                                bool success = true;
                                //Loopt alle plaatsingen langs om te kijken of die veilig is om te plaatsen
                                for (int i = 0; i < ProductPlacements.Count; i++)
                                {
                                    ProductPlacement place = ProductPlacements[i];
                                    if (place.GetPoly().IsSafe(placement.GetPoly())) continue;

                                    success = false;
                                    break;
                                }

                                //Kijkt of die ver genoeg van de lijn is
                                if (success && _route != null && _route.Count >= 2)
                                {
                                    (Position p1, Position p2) = placement.GetPoly().MinDistance(_route);
                                    if (p1.Distance(p2) < 150) success = false;
                                }

                                //Als dit allemaal klopt voegd die het product toe;
                                if (success)
                                {
                                    ProductPlacements.Add(placement);

                                    AddToOverview(placement.Product);

                                    Editor.Dispatcher.Invoke(
                                        () =>
                                        {
                                            DrawProduct(placement, ProductPlacements.IndexOf(placement));
                                        }
                                    );
                                }
                            }
                        }
                    }

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
                }
            ).Start();
        }

        /**
         * Plaats het begin punt of het tweede punt van de meetlat
         */
        private void PlacePoint(MouseButtonEventArgs eventArgs)
        {
            Point p = eventArgs.GetPosition(Editor);

            //Als er nog geen begin is of al een volledige lijn zet die het begin punt anders het tweede punt
            if (_origin == null || _secondPoint != null)
            {
                _origin = new Position((int)p.X, (int)p.Y);
                _secondPoint = null;
            }
            else
            {
                _secondPoint = new Position((int)p.X, (int)p.Y);
                Enabled = false;
                OnPropertyChanged();
            }
        }

        private void RenderPolyPlexi() {
            PlexiLines = StringToList(Design.Plexiglass);

            DistancePlexiLines.ForEach(l => l.Remove(Editor));
            DistancePlexiLines.Clear();
            if (DistancePlexiLines == null) return;
            //Tekend de volledige lijn
            foreach (Models.Polygon Pol in PlexiLines) {
                Position p1 = Pol.GetPositions().First();
                Position p2 = Pol.GetPositions().Last();
                DistancePlexiLines.Add(new DistanceLine(p1, p2, "(Plexiglas)"));
            }

            DistancePlexiLines.ForEach(l => l.Add(Editor));
            ProductPlacements.ForEach(p => CheckCorona(p));
            OnPropertyChanged();
        }

        private void PlacePointPlexi(MouseButtonEventArgs eventArgs) {
            Point p = eventArgs.GetPosition(Editor);
            List<Position> _temppositions = new List<Position>();
            if (_origin == null || _secondPoint != null) {
                if (!Design.Room.GetPoly().Inside(new Position((int) p.X, (int) p.Y))) return;
                _origin = new Position((int) p.X, (int) p.Y);
                _secondPoint = null;
            } else {
                if (!Design.Room.GetPoly().Inside(new Position((int) p.X, (int) p.Y))) return;

                _temppositions.Add(_origin);
                _temppositions.Add(new Position((int) p.X, (int) p.Y));

                //if (!Design.Room.GetPoly().Inside(position))
                if (!Design.Room.GetPoly().Inside(new Models.Polygon(_temppositions))) {
                    return;
                }

                _secondPoint = new Position((int) p.X, (int) p.Y);
                Models.Polygon PlexiLine = new Models.Polygon(_temppositions);
                PlexiLines.Add(PlexiLine);
                DistancePlexiLines.Add(_plexiLine);
                //_plexiLine.Remove(Editor);

                //Database conversie
                UpdateDbPlexiglass();

                PlexiEnabled = false;
                RenderPolyPlexi();
            }
        }

        public void UpdateDbPlexiglass() {
            string plexiLinesString = (PlexiLines?.Count ?? 0) == 0 ? "" : PlexiLines
                .Select(p => p.Convert())
                .Aggregate((s1, s2) => $"{s1};{s2}");

            Design.Plexiglass = plexiLinesString;
        }

        public static List<Models.Polygon> StringToList(string polyList) {
            List<Models.Polygon> returnList = new List<Models.Polygon>();
            if (string.IsNullOrEmpty(polyList)) return returnList;
            return polyList.Split(";").Select(s => new Models.Polygon(s)).ToList();
        }

        /**
         * Plaatst een nieuwe hoek punt voor de route
         */
        private void PlaceRoutePoint(MouseEventArgs eventArgs)
        {
            Point p = eventArgs.GetPosition(Editor);
            //De hoeveelheid pixels waar die naar snapt
            int acc = 1;
            //Rond de lijn af op een accuracy
            Position position = new Position((int)Math.Round(p.X / acc) * acc, (int)Math.Round(p.Y / acc) * acc);
            //Als het nieuwe punt buiten de ruimte zit stopt die
            if (!Design.Room.GetPoly().Inside(position)) return;

            //Voegt het nieuwe punt toe aan het begin van de route
            List<Position> positions = new List<Position> { position };
            if (_route != null) positions.AddRange(_route);
            _route = new Models.Polygon(positions);
            RenderRoute();
        }

        /**
         * Tekend de meetlat tussen de 2 locaties
         */
        public void RenderDistance(Position p1, Position p2)
        {
            //Als de lijn nog niet zichtbaar is wordt die zichtbaar
            if (!_distanceLine.Shows) _distanceLine.Add(Editor);
            _distanceLine.P1 = p1;
            _distanceLine.P2 = p2;
        }

        public void RenderPlexiglass(Position p1, Position p2) {
            if (!_plexiLine.Shows) _plexiLine.Add(Editor);
            _plexiLine.P1 = p1;
            _plexiLine.P2 = p2;
        }

        public void HandleMouseMove(MouseEventArgs eventArgs)
        {
            if (eventArgs.RightButton == MouseButtonState.Pressed)
            {
                Point mousePosition = eventArgs.GetPosition(Editor);
                Vector delta = Point.Subtract(mousePosition, _initialMousePosition);
                var translate = new TranslateTransform(delta.X, delta.Y);
                _transform.Matrix = translate.Value * _transform.Matrix;

                Editor.RenderTransform = _transform;
            }

            //Tekend tijdelijk de lijn voor waar de muis nu is
            Point p = eventArgs.GetPosition(Editor);
            if (Enabled && _origin != null) {
                RenderDistance(_origin, _secondPoint ?? new Position((int)p.X, (int)p.Y));
            }

            if (PlexiEnabled && _origin != null) {
                RenderPlexiglass(_origin, _secondPoint ?? new Position((int) p.X, (int) p.Y));
            }
        }

        /**
         * De graaf waaring alle lijnen worden opgeslagen
         */
        private readonly Dictionary<ProductPlacement, Dictionary<ProductPlacement, DistanceLine>> _lines =
            new Dictionary<ProductPlacement, Dictionary<ProductPlacement, DistanceLine>>();

        //Nette plaatsing die gebruikt wordt voor de route check
        private readonly FakePlacement _fakeRoute = new FakePlacement();

        /**
         * Verwijderd alle corona lijnen tussen deze plaatsing en alle andere
         */
        public void RemoveCorona(ProductPlacement removed)
        {
            if (removed == null) return;
            if (!_lines.ContainsKey(removed)) return;

            //Gaat door alle lijnen heen waar die ooit gemaakt zijn
            foreach (KeyValuePair<ProductPlacement, DistanceLine> entry in _lines[removed])
            {
                //Verwijderd de connectie naar deze deze plaatsing
                _lines[entry.Key].Remove(removed);
                //Verwijderd deze lijnen van het scherm
                if (entry.Value.Shows) entry.Value.Remove(Editor);
            }

            _lines.Remove(removed);
        }

        /**
         * Controlleerd het veranderde product tegen alle andere plaatsing, hier slaat die skip over
         */
        public void CheckCorona(ProductPlacement changed, ProductPlacement skip = null)
        {
            if (changed?.GetPoly() == null) return;
            if (!_lines.ContainsKey(changed))
            {
                _lines[changed] = new Dictionary<ProductPlacement, DistanceLine>();
            }

            //Voegd route toe aan de placements zodat deze ook gecontrolleerd wordt
            List<ProductPlacement> toCheck = ProductPlacements.ToList();
            if (_route != null && _route.Count >= 2) toCheck.Add(_fakeRoute);

            //Gaat door alle producten heen behalve zichzelf en skip
            foreach (ProductPlacement placement in toCheck)
            {
                if (Equals(placement, changed) || (skip != null && Equals(placement, skip)) ||
                    placement.GetPoly() == null) continue;

                //Controlleerd door middel van snelle minder accuraten functies hoe het nodig is om te checken
                (bool needed, bool safe) = placement.GetPoly().PreciseNeeded(changed.GetPoly(), 150);
                if (!needed && safe) continue;

                //Kijkt accuraat wat de afstand is
                (Position p1, Position p2) = placement.GetPoly().MinDistance(changed.GetPoly());
                Models.Line lin = new Models.Line(p1, p2);
                bool plexiCheck = PlexiLines
                    .Select(poly => poly.GetLines().First())
                    .Any(polyline => lin.IntersectionLineSegment(polyline) != null);

                //Maakt een nieuwe lijn aan als deze nog niet bestond en anders pakt die de oude lijn
                DistanceLine line = _lines[changed].ContainsKey(placement)
                    ? _lines[changed][placement]
                    : new DistanceLine(null, null);

                //Vervangt de lijn met de vorige
                _lines[changed][placement] = line;

                //Als de mee vergelijken nog niet bestaat wordt deze aan gemaakt
                if (!_lines.ContainsKey(placement))
                    _lines[placement] = new Dictionary<ProductPlacement, DistanceLine>();
                //Zet de referencitie naar de lijn vanaf de andere kant, dit maakt een soort graaf
                _lines[placement][changed] = line;

                //Als het binnen de bepaalde afstand zit wordt de lijn getekend en anders weggehaalt
                if (p1.Distance(p2) >= 150 || plexiCheck)
                {
                    line.Remove(Editor);
                }
                else
                {
                    line.P1 = p1;
                    line.P2 = p2;

                    if (!line.Shows) line.Add(Editor);
                }
            }
        }

        public void SetDesign(Design design)
        {
            // Haalt het design uit de database
            Design = DesignService.Instance.Get(design.Id);
            ProductPlacements = design.ProductPlacements;
            ProductPlacements ??= new List<ProductPlacement>();
            RoomPlacements = design.Room.RoomPlacements;
            ProductPlacements ??= new List<ProductPlacement>();
            _productOverview = new Dictionary<Product, ProductData>();
            //Wanneer niet in test env render die de ruimte
            if (Editor != null)
            {
                // Sets the dimensions of the current room
                SetRoomDimensions();
                RenderRoom();

                RenderRoomFrames();

                RenderPolyPlexi();
                //Tekend de route en alle corona lijnen
                ProductPlacements.ForEach(p => CheckCorona(p));
                RenderRoute();

                // Zet de schaal van de ruimte op basis van de dimensies, dit moet na het zetten van het design
                SetRoomScale();
            }
            
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void PlaceProduct(Product product, int x, int y)
        {
            if (Editor != null) RemoveCorona(_tempPlacement);
            // Checkt of het product niet null is en of de foto geplaatst mag worden
            if (product == null || !AllowDrop) return;

            ProductPlacement placement = new ProductPlacement(x, y, product);
            ProductPlacements.Add(placement);

            // Add product to product overview
            AddToOverview(product);
            if (Editor != null) DrawProduct(placement, ProductPlacements.IndexOf(placement));
            if (Editor != null) CheckCorona(placement);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void TryToMoveProduct(ProductPlacement placement, int newX, int newY)
        {
            //Verwijderd de corona lijnen van de preview
            RemoveCorona(_tempPlacement);
            //Alleen als een object naar het nieuwe punt verplaatst mag worden, wordt het vervangen.
            if (!AllowDrop)
            {
                //Tekend de corona lijnen van de orginele plaatsin
                CheckCorona(placement);
                return;
            }

            //Verwijder de placement van de placement om te voorkomen dat het product verdubbeld wordt
            var index = ProductPlacements.FindIndex(element => Equals(element, placement));
            //Trek de helft van de hoogte en breedte van het product eraf
            //Zodat het product in het midden van de cursor staat
            placement.X = newX;
            placement.Y = newY;

            //Verwijderd de corona lijnen van de preview
            RemoveCorona(ProductPlacements[index]);
            //Na het aanpassen wordt het weer toegevoegd om de illusie te geven dat het in de lijst wordt aangepast
            ProductPlacements[index] = placement;
            RenderRoom();
            //Tekend de nieuw lijnen van de plaatsing
            CheckCorona(placement);
        }

        public void CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            //Rechtermuisknop zorgt ervoor dat informatie over het product wordt getoond
            if (e.ChangedButton == MouseButton.Right)
            {
                _initialMousePosition = e.GetPosition(Editor);

                //Kaapt het event als er op een hoekpunt van de route is geklikt
                if (RouteEnabled && sender is Ellipse ellipse)
                {
                    int pos = int.Parse(ellipse.Uid);

                    //Verwijderd dit hoekpunt
                    List<Position> positions = _route.ToList();
                    positions.RemoveAt(pos);
                    _route = new Models.Polygon(positions);
                    RenderRoute();

                    return;
                }

                if (sender is Line) {
                    var line = (System.Windows.Shapes.Line) sender;
                    Position p1 = new Position((int) line.X1, (int) line.Y1);
                    Position p2 = new Position((int) line.X2, (int) line.Y2);

                    int index = DistancePlexiLines.FindIndex(i => i.P1.Equals(p1) && i.P2.Equals(p2));

                    if (index == -1) {
                        return;
                    }
                    
                    PlexiLines.RemoveAt(index);
                    UpdateDbPlexiglass();

                    RenderPolyPlexi();
                    //Editor.Children.Remove();
                }

                if (sender.GetType() == typeof(Canvas))
                {
                    _selectedPlacement = null;
                    RenderRoom();
                }

                if (sender.GetType() != typeof(Image)) return;
                var image = sender as Image;
                var placement = ProductPlacements.Where(
                    placement =>
                        placement.X == Canvas.GetLeft(image) && placement.Y == Canvas.GetTop(image)
                );
                if (placement.Count() > 0)
                {
                    _selectedPlacement = placement.First();
                }

                RenderRoom();
            }
            //Linkermuisknop betekend dat het product wordt verplaatst
            else
            {
                //Als meetlat aanstaat vervangt die deze behavivoer
                if (Enabled)
                {
                    PlacePoint(e);
                    return;
                }

                //Kaapt het event als looproute aan staat
                if (RouteEnabled)
                {
                    if (sender is Ellipse)
                    {
                        //Verplaatst het hoekpunt waar op geklikt is
                        DragDrop.DoDragDrop(Editor, sender, DragDropEffects.Move);
                    }
                    else
                    {
                        //Plaats een hoekpunt op de plek waar geklikt is
                        PlaceRoutePoint(e);
                    }

                    return;
                }

                if (PlexiEnabled) {
                    PlacePointPlexi(e);
                    return;
                }


                if (sender.GetType() != typeof(Image)) return;
                var image = sender as Image;
                var placement = ProductPlacements.Where(
                    placement =>
                        placement.X == Canvas.GetLeft(image) && placement.Y == Canvas.GetTop(image)
                );
                if (placement.Count() > 0)
                {
                    _draggingPlacement = placement.First();
                    DragDrop.DoDragDrop(Editor, _draggingPlacement, DragDropEffects.Move);
                }
            }

            // Toegevoegd zodat de corona score wordt bijgewerkt
            // TODO: kan mogelijk beter
            OnPropertyChanged();
        }

        public void CatalogusMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Linker muisknop moet ingdrukt zijn
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender.GetType() != typeof(Image)) return;
                // Cast datacontext naar product
                var obj = (Product)((Image)sender).DataContext;
                // Init drag & drop voor geselecteerde product
                DragDrop.DoDragDrop(Editor, obj, DragDropEffects.Link);
            }
        }

        public void CanvasDragDrop(object sender, DragEventArgs e)
        {
            //Als er geen product is geselecteerd, doe niks
            if (e.Data == null) return;
            if (e.Data.GetDataPresent(typeof(Ellipse)))
            {
                //Als er een hoek punt van een route wordt versleept kijkt die of deze in de ruimte zit en zo ja past die hem aan
                Position position = new Position((int)e.GetPosition(Editor).X, (int)e.GetPosition(Editor).Y);
                if (!Design.Room.GetPoly().Inside(position)) return;
                int pos = int.Parse(((Ellipse)e.Data.GetData(typeof(Ellipse))).Uid);
                List<Position> positions = _route.ToList();
                positions[pos] = position;
                _route = new Models.Polygon(positions);
                RenderRoute();
                return;
            }

            //In dit geval wordt er een product toegevoegd
            if (e.Data.GetDataPresent(typeof(Product)))
            {
                var selectedProduct = (Product)e.Data.GetData(typeof(Product));
                Point position = e.GetPosition(Editor);
                //Trek de helft van de hoogte en breedte van het product eraf
                //Zodat het product in het midden van de cursor staat
                PlaceProduct(
                    selectedProduct,
                    (int)(position.X - (selectedProduct.Width / 2)),
                    (int)(position.Y - (selectedProduct.Length / 2))
                );
                RenderRoom();
            }

            //Hier wordt een product dat al in het design zit verplaatst
            else if (e.Data.GetDataPresent(typeof(ProductPlacement)))
            {
                ProductPlacement placement = (ProductPlacement)e.Data.GetData(typeof(ProductPlacement));
                Point position = e.GetPosition(Editor);
                int x = (int)position.X - (placement.GetPoly().Width / 2);
                int y = (int)position.Y - (placement.GetPoly().Length / 2);

                TryToMoveProduct(placement, x, y);
                _draggingPlacement = null;
                RenderRoom();
            }
        }

        private readonly ProductPlacement _tempPlacement = new ProductPlacement();

        private (Image i, Rectangle r)? _prevPlace;

        public void CanvasDragOver(object sender, DragEventArgs e)
        {
            //Controleer of er een product is geselecteerd
            if (e.Data == null) return;
            if (e.Data.GetDataPresent(typeof(Ellipse)))
            {
                CanvasDragDrop(sender, e);
                return;
            }

            Product selectedProduct = null;
            ProductPlacement skip = null;
            int rotation = 0;
            //Afhankelijk van het type data wordt de product op een andere manier opgehaald

            //Haal de positie van de cursor op
            Point position = e.GetPosition(Editor);

            if (e.Data.GetDataPresent(typeof(Product)))
            {
                selectedProduct = (Product)e.Data.GetData(typeof(Product));
            }
            else if (e.Data.GetDataPresent(typeof(ProductPlacement)))
            {
                ProductPlacement placement = (ProductPlacement)e.Data.GetData(typeof(ProductPlacement));
                skip = placement;
                selectedProduct = placement?.Product;
                rotation = placement?.Rotation ?? 0;
            }

            //Als de muis niet bewogen is hoeft het niet opnieuw getekend te worden
            if (position == _previousPosition) return;
            _previousPosition = position;

            _tempPlacement.Product = selectedProduct;
            _tempPlacement.Rotation = rotation;
            _tempPlacement.X = (int)position.X - _tempPlacement.GetPoly().Width / 2;
            _tempPlacement.Y = (int)position.Y - _tempPlacement.GetPoly().Length / 2;

            //Checkt alleen voor afstand bij de nodige producten
            RemoveCorona(skip);
            CheckCorona(_tempPlacement, skip);

            // Check of het product in de ruimte wordt geplaatst
            AllowDrop = CheckRoomCollisions(position, selectedProduct) &&
                        CheckProductCollisions(_tempPlacement);

            //Teken de ruimte en de al geplaatste producten
            // RenderRoom();
            // Render het plaatje vna het product als de cursor binnen de polygon zit
            if (_prevPlace != null)
            {
                Editor.Children.Remove(_prevPlace.Value.i);
                Editor.Children.Remove(_prevPlace.Value.r);
            }

            if (skip != null && _images.ContainsKey(skip)) _images[skip].Opacity = 0.5;

            _prevPlace = DrawProduct(
                _tempPlacement,
                200,
                !AllowDrop,
                rotation
            );
        }

        private PlacementSelectScreen _screen;

        /**
         * Rendert alle element in de ruimte opnieuw<br/>
         * <b>Dit is erg intensief met veel producten</b>
         */
        private void RenderRoom()
        {
            //Verwijder alle oude element op het canvas
            if (_screen != null)
            {
                Editor.Children.Remove(_screen);
                _screen = null;
            }

            foreach (Image image in _images.Values)
            {
                Editor.Children.Remove(image);
            }

            foreach (Rectangle rect in _rectangles.Values)
            {
                Editor.Children.Remove(rect);
            }

            _images.Clear();
            _rectangles.Clear();

            for (int i = 0; i < ProductPlacements.Count; i++)
            {
                var placement = ProductPlacements[i];
                //Controleer of de placement op dat moment verplaatst wordt
                //Als dit het geval is moet de placement doorzichtig worden
                DrawProduct(
                    placement, i, _draggingPlacement == placement, placement.Rotation
                );
            }

            if (_selectedPlacement != null)
            {
                DrawSelectionButtons(_selectedPlacement);
            }
        }

        private void DrawSelectionButtons(ProductPlacement placement)
        {
            PlacementSelectScreen selectScreen = new PlacementSelectScreen();
            _screen = selectScreen;
            selectScreen.DataContext = placement.Product;
            // Verwijderd de plaatsing en rendert de ruimte opnieuw
            selectScreen.DeleteButton.Click += delegate
            {
                ProductPlacements.Remove(placement);
                _selectedPlacement = null;
                // Toegevoegd zodat de corona score wordt bijgewerkt
                OnPropertyChanged();
                RemoveCorona(placement);
                Editor.Children.Remove(selectScreen);
                _screen = null;
                //Verwijder de opgeslagen elementen
                Editor.Children.Remove(_images[placement]);
                Editor.Children.Remove(_rectangles[placement]);
                _images.Remove(placement);
                _rectangles.Remove(placement);
            };
            // Sluit de placementselect scherm
            selectScreen.CloseButton.Click += delegate
            {
                _selectedPlacement = null;
                Editor.Children.Remove(selectScreen);
            };
            // Roteert het product naar links
            selectScreen.RotateLeftButton.Click += delegate
            {
                placement.Rotation = placement.Rotation == 0 ? 270 : placement.Rotation -= 90;
                RenderRoom();
            };
            // Roteert het product naar rechts
            selectScreen.RotateRightButton.Click += delegate
            {
                placement.Rotation = placement.Rotation == 270 ? 0 : placement.Rotation += 90;
                RenderRoom();
            };
            Canvas.SetTop(selectScreen, placement.Y + placement.GetPoly().Length);
            Canvas.SetLeft(selectScreen, placement.X);
            Panel.SetZIndex(selectScreen, 300);
            Editor.Children.Add(selectScreen);
        }

        public Dictionary<ProductPlacement, Image> _images = new Dictionary<ProductPlacement, Image>();
        public Dictionary<ProductPlacement, Rectangle> _rectangles = new Dictionary<ProductPlacement, Rectangle>();

        public (Image i, Rectangle r) DrawProduct(
            ProductPlacement placement,
            int? placementIndex = null,
            bool transparent = false,
            int rotation = 0
        )
        {
            //Haal de bestandsnaam van de foto op of gebruik de default
            Product product = placement.Product;
            int x = placement.X;
            int y = placement.Y;

            var photo = product.Photo ?? "placeholder.png";
            var actualWidth = rotation % 180 == 0 ? product.Width : product.Length;
            var actualLength = rotation % 180 == 0 ? product.Length : product.Width;
            // Veranderd de rotatie van het product
            TransformedBitmap tempBitmap = new TransformedBitmap();

            tempBitmap.BeginInit();
            var source = new BitmapImage(new Uri(Environment.CurrentDirectory + $"/Resources/Images/{photo}"));
            tempBitmap.Source = source;
            RotateTransform transform = new RotateTransform(rotation, source.Width / 2, source.Height / 2);
            tempBitmap.Transform = transform;
            tempBitmap.EndInit();

            var image = new Image
            {
                Source = tempBitmap,
                Height = actualLength,
                Width = actualWidth
            };

            //Als transparent in als parameter naar true wordt gezet wordt de afbeelding doorzichtig
            if (transparent) image.Opacity = 0.5;


            Canvas.SetTop(image, y);
            Canvas.SetLeft(image, x);
            // Voeg product toe aan canvas
            Editor.Children.Add(image);

            var rect = new Rectangle()
            {
                Stroke = Brushes.Red,
                Height = actualLength,
                Width = actualWidth,
            };

            //Slaat de rectangle op met placement als key zodat die makkerlijker te verwijderen is
            _rectangles[placement] = rect;

            Canvas.SetTop(rect, y);
            Canvas.SetLeft(rect, x);
            Editor.Children.Add(rect);
            // Voegt het id van het productplacement index in de productplacement list
            image.Uid ??= placementIndex.ToString();

            //Slaat de foto op met placement als key zodat die makkerlijker te verwijderen is
            _images[placement] = image;


            return (image, rect);
        }

        public static List<Product> LoadProducts() { return ProductService.Instance.GetAll(); }

        public void AddToOverview(Product product)
        {
            var price = product.Price ?? 0.0;
            if (_productOverview.ContainsKey(product))
            {
                _productOverview[product].Total = _productOverview[product].Total + 1;
                _productOverview[product].TotalPrice = Math.Round(_productOverview[product].TotalPrice + price, 2);
            }
            else
            {
                _productOverview.Add(product, new ProductData() { Total = 1, TotalPrice = price });
            }
        }

        public void SetRoomDimensions()
        {
            var coordinates = Room.ToList(Design.Room.Positions);

            PointCollection points = new PointCollection();
            // Voeg de punten toe aan een punten collectie
            for (int i = 0; i < coordinates.Count; i++)
            {
                points.Add(new Point(coordinates[i].X, coordinates[i].Y));
                DistanceLine line = new DistanceLine(coordinates[i], coordinates[(i + 1) % coordinates.Count]);
                line.Add(Editor);
            }
            
            ImageBrush imgBrush = new ImageBrush();  
            imgBrush.ImageSource = new BitmapImage(new Uri( "../../../Resources/Images/Assets/BluePrint.png" , UriKind.Relative)); 

            RoomPoly.Stroke = Brushes.Black;
            //RoomPoly.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4A6de5"));
            RoomPoly.Fill = imgBrush;
            RoomPoly.StrokeThickness = 1;
            RoomPoly.HorizontalAlignment = HorizontalAlignment.Left;
            RoomPoly.VerticalAlignment = VerticalAlignment.Center;
            RoomPoly.Points = points;
            Editor.Children.Add(RoomPoly);
        }

        public bool CheckRoomCollisions(Point point, Product product)
        {
            int yOffset = product.Length / 2;
            int xOffset = product.Width / 2;

            Models.Polygon p = product.GetPoly().Offset((int) point.X - xOffset, (int) point.Y - yOffset);

            foreach (RoomPlacement placement in Design.Room.RoomPlacements) {
                if (placement.GetPoly().DoesCollide(p)) return false;
            }

            return Design.Room.GetPoly()
                .Inside(p);
        }

        public bool CheckProductCollisions(ProductPlacement placement)
        {
            Models.Polygon poly = placement.GetPoly();
            foreach (ProductPlacement p in ProductPlacements)
            {
                if (Equals(p, _draggingPlacement)) continue;
                if (p.GetPoly().DoesCollide(poly))
                {
                    return false;
                }
            }

            return true;
        }

        public void SetRoomScale()
        {
            double scale;

            // Zet de dimensies van de ruimte polygon
            RoomPoly.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            // Als de breedte hoger is dan de breedte wordt de breedte gebruikt voor de schaal en vice versa
            if (RoomPoly.DesiredSize.Width > RoomPoly.DesiredSize.Height)
            {
                scale = (Navigator.Instance.CurrentPage.ActualWidth - 260) / RoomPoly.DesiredSize.Width;
            }
            else
            {
                scale = (Navigator.Instance.CurrentPage.ActualHeight - 20) / RoomPoly.DesiredSize.Height;
            }

            ScaleCanvas(scale, _initialMousePosition);
        }

        public void CanvasMouseScroll(object sender, MouseWheelEventArgs e)
        {
            Point mousePosition = e.GetPosition(Editor);

            double scaleFactor = 1.05;
            if (e.Delta < 0)
            {
                scaleFactor = 1 / scaleFactor;
            }

            ScaleCanvas(scaleFactor, mousePosition);
        }

        private void ScaleCanvas(double scale, Point mousePosition)
        {
            // Kijkt of de gegeven schaal binnen de pagina past, zo niet veranderd de schaal niet
            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(scale, scale, mousePosition.X, mousePosition.Y);
            _transform.Matrix = scaleMatrix;
            Editor.RenderTransform = _transform;
        }

        private void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static string FromList(List<DistanceLine> distancelines) {
            string returnstring = "";
            if (distancelines == null) return null;
            //IEnumerable<Position> enumerable = positions.ToList();
            foreach (DistanceLine position in distancelines) {
                returnstring = $"{returnstring}{position.P1};{position.P2}|";
            }

            return returnstring;
        }

        public static List<DistanceLine> ToList(string DistanceLines) {
            switch (DistanceLines) {
                case null:
                case "": {
                    return new List<DistanceLine>();
                }
                default: {
                    var list = new List<DistanceLine>();
                    var lines = DistanceLines.Split("|").ToList();
                    foreach (var line in lines) {
                        if (line == "") continue;
                        var positions = line.Split(";")
                            .Select(p => p.Split(",").Select(Int32.Parse).ToList())
                            .Select(p => new Position(p[0], p[1]))
                            .ToList();
                        list.Add(new DistanceLine(positions[0], positions[1]));
                    }

                    return list;
                }
            }

            ;
        }
    }

    public class ProductData
    {
        public int Total { get; set; }
        public double TotalPrice { get; set; }
    }

    public class DistanceLine
    {
        private Line _line;
        private Line _line2;
        private TextBlock _textBlock;
        private string _prefix;
        private Position _p1;
        private Position _p2;

        public Position P1
        {
            get => _p1;
            set
            {
                _p1 = value;
                UpdatePositions();
            }
        }

        public Position P2
        {
            get => _p2;
            set
            {
                _p2 = value;
                UpdatePositions();
            }
        }

        public bool Shows { get; private set; }

        public DistanceLine(Position p1, Position p2, string prefix = "")
        {
            _p1 = p1;
            _p2 = p2;
            _line = new Line();
            _line2 = new Line();
            _textBlock = new TextBlock();
            _prefix = prefix;


            _line.Stroke = Brushes.White;
            _line.StrokeThickness = 3;
            _line2.Stroke = Brushes.Black;
            _line2.StrokeThickness = 1;
            _textBlock.Foreground = new SolidColorBrush(Colors.Black);
            _textBlock.Background = new SolidColorBrush(Colors.White);
        }

        /**
         * Renders the line on the canvas
         */
        public void Add(Canvas editor)
        {
            Shows = true;
            editor.Children.Add(_line);
            editor.Children.Add(_line2);
            editor.Children.Add(_textBlock);
            Render();
        }

        /**
         * Set stuff that needs the canvas to be used
         */
        private void Render()
        {
            Panel.SetZIndex(_line, 100);
            Panel.SetZIndex(_line2, 101);
            Panel.SetZIndex(_textBlock, 102);

            UpdatePositions();
        }

        /**
         * Updates the lines so that they reflect P1 and P2
         */
        private void UpdatePositions()
        {
            if (P1 == null || P2 == null) return;
            _line.X1 = P1.X;
            _line.Y1 = P1.Y;
            _line.X2 = P2.X;
            _line.Y2 = P2.Y;

            _line2.X1 = P1.X;
            _line2.Y1 = P1.Y;
            _line2.X2 = P2.X;
            _line2.Y2 = P2.Y;

            Position center = P1.Center(P2);
            _textBlock.Text = _prefix + FormatText(P1.Distance(P2));
            Size size = MeasureString();

            double dx = size.Width / 2;

            //Calculate the rotation by use of atan2
            double radians = Math.Atan2(P2.Y - P1.Y, P2.X - P1.X);
            double degrees = ConvertRadiansToDegrees(radians);
            _textBlock.RenderTransform = new RotateTransform(degrees, dx, 0);

            Canvas.SetLeft(_textBlock, center.X - dx);
            Canvas.SetTop(_textBlock, center.Y - 0);
        }

        /**
         * Measaures the length of the text, this is needed to properly rotate the text block
         */
        private Size MeasureString()
        {
            var formattedText = new FormattedText(
                _textBlock.Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    _textBlock.FontFamily, _textBlock.FontStyle, _textBlock.FontWeight, _textBlock.FontStretch
                ),
                _textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1
            );

            return new Size(formattedText.Width, formattedText.Height);
        }

        /**
         * Generate the text is the correct format
         */
        private static string FormatText(double distance)
        {
            if (distance < 100)
            {
                return distance.ToString("F0") + " cm";
            }

            return (distance / 100).ToString("F2") + " m";
        }

        /**
         * Removes the lines and text from the canvas
         */
        public void Remove(Canvas editor)
        {
            Shows = false;
            editor.Children.Remove(_line);
            editor.Children.Remove(_line2);
            editor.Children.Remove(_textBlock);
        }

        /**
         * Converts radians to degrees
         */
        private static double ConvertRadiansToDegrees(double radians)
        {
            double degrees = 180 / Math.PI * radians;
            if (degrees > 90) return degrees + 180;
            if (degrees < -90) return degrees + 180;
            return degrees;
        }
    }

    //Holder for a polygon as placement, so this works with the distance check method
    internal class FakePlacement : ProductPlacement
    {
        public Models.Polygon poly { get; set; }

        public override Models.Polygon GetPoly() => poly;
    }
}