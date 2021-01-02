using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Designer.Utils;
using Designer.View;
using Designer.View.Components;
using Models;
using Models.Utils;
using Services;
using Line = Models.Line;
using Polygon = Models.Polygon;

namespace Designer.ViewModel {
    public class DesignEditor : INotifyPropertyChanged {
        //Nette plaatsing die gebruikt wordt voor de route check
        private readonly FakePlacement _fakeRoute = new FakePlacement();

        /**
         * De graaf waaring alle lijnen worden opgeslagen
         */
        internal readonly Dictionary<ProductPlacement, Dictionary<ProductPlacement, DistanceLine>> _lines =
            new Dictionary<ProductPlacement, Dictionary<ProductPlacement, DistanceLine>>();

        private readonly ProductPlacement _tempPlacement = new ProductPlacement();
        private readonly DistanceLine _distanceLine;
        private ProductPlacement _draggingPlacement;
        private readonly List<Ellipse> _ellipses = new List<Ellipse>();

        public Dictionary<ProductPlacement, Image> _images = new Dictionary<ProductPlacement, Image>();
        private Point _initialMousePosition;

        private Position _origin;

        //private Position _pSecondPoint;
        private readonly DistanceLine _plexiLine;
        private Point _previousPosition;

        private (Image i, Rectangle r)? _prevPlace;
        public Dictionary<ProductPlacement, Rectangle> _rectangles = new Dictionary<ProductPlacement, Rectangle>();

        private readonly List<DistanceLine> _routeLines = new List<DistanceLine>();

        private PlacementSelectScreen _screen;
        private Position _secondPoint;
        private ProductPlacement _selectedPlacement;
        public bool AllowDrop;
        private readonly TransformGroup bothTransforms;
        public List<DistanceLine> DistancePlexiLines = new List<DistanceLine>();
        private readonly TranslateTransform panTransform;

        public List<Polygon> PlexiLines = new List<Polygon>();

        public double Scale = 1.0;
        private readonly ScaleTransform zoomTransform;

        //Special constructor for unit tests
        public DesignEditor(Design design) {
            SetDesign(design);
            Products = LoadProducts();
        }

        public DesignEditor() {
            GotoDesigns = new PageCommand(
                () => {
                    ViewDesignsView DesignCatalog = new ViewDesignsView();
                    DesignCatalog.DesignSelected += (o, e) => {
                        Navigator.Instance.Replace(new DesignEditorView(e.Value));
                    };
                    return DesignCatalog;
                }
            );
            Products = LoadProducts();
            Editor = new Canvas();

            RoomPoly = new System.Windows.Shapes.Polygon();
            CatalogusMouseDownCommand =
                new ArgumentCommand<MouseButtonEventArgs>(e => CatalogusMouseDown(e.OriginalSource, e));
            CanvasMouseDownCommand =
                new ArgumentCommand<MouseButtonEventArgs>(e => CanvasMouseDown(e.OriginalSource, e));
            DragDropCommand = new ArgumentCommand<DragEventArgs>(e => CanvasDragDrop(e.OriginalSource, e));
            DragOverCommand = new ArgumentCommand<DragEventArgs>(e => CanvasDragOver(e.OriginalSource, e));
            MouseMoveCommand = new ArgumentCommand<MouseEventArgs>(HandleMouseMove);
            Measure = new BasicCommand(StartMeasure);
            Plexiglass = new BasicCommand(StartPlexiglass);
            Layout = new BasicCommand(this.GenerateLayout);
            GenerateRoute = new BasicCommand(this.GenerateWalkRoute);
            GeneratePlexiline = new BasicCommand(this.GeneratePlexi);
            RemoveRoute = new BasicCommand(DeleteRoute);
            RemovePlexiglass = new BasicCommand(DeletePlexiglass);
            ClearProducts = new BasicCommand(Clear);
            Save = new BasicCommand(() => DesignService.Instance.SaveChanges());
            CanvasMouseScrollCommand =
                new ArgumentCommand<MouseWheelEventArgs>(e => CanvasMouseScroll(e.OriginalSource, e));
            _productOverview = new Dictionary<Product, ProductData>();

            _distanceLine = new DistanceLine(null, null);
            _plexiLine = new DistanceLine(null, null, "(Plexiglas)");

            panTransform = new TranslateTransform();
            zoomTransform = new ScaleTransform();
            bothTransforms = new TransformGroup();

            bothTransforms.Children.Add(panTransform);
            bothTransforms.Children.Add(zoomTransform);

            Editor.RenderTransform = bothTransforms;
        }

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
        public BasicCommand RemovePlexiglass { get; set; }
        public ArgumentCommand<MouseWheelEventArgs> CanvasMouseScrollCommand { get; set; }
        public Product SelectedProduct => _selectedPlacement.Product;
        public Design Design { get; set; }
        public Canvas Editor { get; set; }
        public System.Windows.Shapes.Polygon RoomPoly { get; set; }

        public int DistanceScore {
            get {
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
                int m = distanceLines.Count == 0 ? 100 : (int) (count / distanceLines.Count * 100);
                m = Math.Min(0, m);
                m = Math.Max(100, m);
                //                                                                             Groen       Rood
                DistanceColour = (SolidColorBrush) new BrushConverter().ConvertFrom(m == 100 ? "#00D092" : "#d00037");
                OnPropertyChanged("DistanceColour");

                return m;
            }
        }

        public int VentilationScore {
            get {
                // als er niks is 0
                if (RoomPlacements == null || RoomPlacements.Count == 0) return 0;
                // minder dan 2 is ook een onvoldoende
                if (RoomPlacements.Count < 2) return 0;
                // maakt lijst van x en y coordinaten (+1) voor de calculatie (voorbereiding Pick's theorem)
                List<Position> roompositions = new List<Position>();
                roompositions = Room.ToList(Design.Room.Positions);
                List<int> xlist = new List<int>();
                roompositions.ForEach(l => xlist.Add(l.X));
                List<int> ylist = new List<int>();
                roompositions.ForEach(l => ylist.Add(l.Y));
                // 1x extra toevoegen omdat de laatste x de eerste moet
                xlist.Add(roompositions.First().X);
                ylist.Add(roompositions.First().Y);
                int i = 0;
                double cm2 = 0;
                // cm2 calculatie (Pick's theorem)
                foreach (Position pos in roompositions) {
                    cm2 += (xlist[i] * ylist[i + 1] - ylist[i] * xlist[i + 1]) / 2;
                    i++;
                }

                // convert negatief getal naar positief
                if (cm2 < 0) cm2 = cm2 * -1;
                int m = (int) (cm2 / 10000);
                m = 100 - (m - RoomPlacements.Count);

                m = Math.Min(0, m);
                m = Math.Max(100, m);
                //                                                                                Groen       Rood
                VentilationColour = (SolidColorBrush) new BrushConverter().ConvertFrom(m > 55 ? "#00D092" : "#d00037");

                OnPropertyChanged("VentilationColour");

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
                int m = distanceLines.Count == 0 ? 100 : (int) (count / distanceLines.Count * 100);

                m = Math.Max(0, m);
                m = Math.Min(100, m);
                //                                                                            Groen       Rood
                RouteColour = (SolidColorBrush) new BrushConverter().ConvertFrom(m == 100 ? "#00D092" : "#d00037");
                OnPropertyChanged("RouteColour");

                return m;
            }
        }

        public Brush DistanceColour { get; set; }
        public Brush VentilationColour { get; set; }
        public Brush RouteColour { get; set; }
        private double _canvasHeight => Navigator.Instance.CurrentPage.ActualHeight - 20;

        private double _canvasWidth => Navigator.Instance.CurrentPage.ActualWidth - 260;

        public bool PlexiEnabled { get; set; }

        public bool Enabled { get; set; }

        public bool RouteEnabled { get; set; }

        internal Polygon _route {
            get => Design.GetRoutePoly();
            set => Design.Route = value?.Convert();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //Verwijder de lijn als er al een meetlat was
        public void StartMeasure() {
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

        /**
         * Verwijderd de route
         */
        public void DeleteRoute() {
            _route = null;

            RemoveCorona(_fakeRoute);
            RenderRoute();
        }

        /**
         * Tekend de volledige route
         */
        public void RenderRoute() {
            //Verwijderd eerst de volledige lijn
            _routeLines.ForEach(l => l.Remove(Editor));
            _routeLines.Clear();
            _ellipses.ForEach(Editor.Children.Remove);
            _ellipses.Clear();
            if (_route == null || _route.Count == 0) return;
            //Tekend de volledige lijn
            foreach (Line line in _route.GetLines()) _routeLines.Add(new DistanceLine(line.P1, line.P2));

            //Tekend alle hoek punten
            for (int index = 0; index < _route.Count; index++) {
                Position position = _route[index];
                int size = 10;
                Ellipse ellipse = new Ellipse {
                    Height = size,
                    Width = size,
                    Fill = Brushes.Black,
                    Uid = index.ToString()
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
            _fakeRoute.Poly = _route;
            CheckCorona(_fakeRoute);
        }

        /**
         * Verwijderd alle plaatsingen
         */
        public void Clear() {
            ProductPlacements.ForEach(RemoveCorona);
            ProductPlacements.Clear();
            OnPropertyChanged();
            RenderRoom();
        }

        /**
         * Plaatst alle deuren en ramen die in de ruimte zitten
         */
        public void RenderRoomFrames() {
            if (Design.Room.RoomPlacements != null)
                foreach (RoomPlacement frame in Design.Room.RoomPlacements) {
                    Position pos = RoomPlacement.ToPosition(frame.Positions);
                    System.Windows.Shapes.Polygon newPoly = new System.Windows.Shapes.Polygon();

                    if (frame.Type == FrameTypes.Door) {
                        int x = pos.X;
                        int y = pos.Y;

                        if (frame.Rotation == 0) y -= 25;
                        if (frame.Rotation == 270) x -= 25;

                        PointCollection points = new PointCollection {
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

                        System.Windows.Shapes.Line window = new System.Windows.Shapes.Line {
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

        /**
         * Plaats het begin punt of het tweede punt van de meetlat
         */
        private void PlacePoint(MouseButtonEventArgs eventArgs) {
            Point p = eventArgs.GetPosition(Editor);

            //Als er nog geen begin is of al een volledige lijn zet die het begin punt anders het tweede punt
            if (_origin == null || _secondPoint != null) {
                _origin = new Position((int) p.X, (int) p.Y);
                _secondPoint = null;
            } else {
                _secondPoint = new Position((int) p.X, (int) p.Y);
                Enabled = false;
                OnPropertyChanged();
            }
        }

        internal void RenderPolyPlexi() {
            PlexiLines = StringToList(Design.Plexiglass);

            DistancePlexiLines.ForEach(l => l.Remove(Editor));
            DistancePlexiLines.Clear();
            if (DistancePlexiLines == null) return;
            //Tekend de volledige lijn
            foreach (Polygon Pol in PlexiLines) {
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
                if (!Design.Room.GetPoly().Inside(new Polygon(_temppositions))) return;

                _secondPoint = new Position((int) p.X, (int) p.Y);
                Polygon PlexiLine = new Polygon(_temppositions);
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
            string plexiLinesString = (PlexiLines?.Count ?? 0) == 0
                ? ""
                : PlexiLines
                    .Select(p => p.Convert())
                    .Aggregate((s1, s2) => $"{s1};{s2}");

            Design.Plexiglass = plexiLinesString;
        }

        public static List<Polygon> StringToList(string polyList) {
            List<Polygon> returnList = new List<Polygon>();
            if (string.IsNullOrEmpty(polyList)) return returnList;
            return polyList.Split(";").Select(s => new Polygon(s)).ToList();
        }

        /**
         * Plaatst een nieuwe hoek punt voor de route
         */
        private void PlaceRoutePoint(MouseEventArgs eventArgs) {
            Point p = eventArgs.GetPosition(Editor);
            //De hoeveelheid pixels waar die naar snapt
            int acc = 1;
            //Rond de lijn af op een accuracy
            Position position = new Position((int) Math.Round(p.X / acc) * acc, (int) Math.Round(p.Y / acc) * acc);
            //Als het nieuwe punt buiten de ruimte zit stopt die
            if (!Design.Room.GetPoly().Inside(position)) return;

            //Voegt het nieuwe punt toe aan het begin van de route
            List<Position> positions = new List<Position> {position};
            if (_route != null) positions.AddRange(_route);
            _route = new Polygon(positions);
            RenderRoute();
        }

        public void DeletePlexiglass() {
            PlexiLines.Clear();
            UpdateDbPlexiglass();
            RenderPolyPlexi();
        }

        /**
         * Tekend de meetlat tussen de 2 locaties
         */
        public void RenderDistance(Position p1, Position p2) {
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

        public void HandleMouseMove(MouseEventArgs eventArgs) {
            if (eventArgs.RightButton == MouseButtonState.Pressed) {
                Point mousePosition = eventArgs.GetPosition(Editor);
                Vector delta = Point.Subtract(mousePosition, _initialMousePosition);
                panTransform.X += delta.X;
                panTransform.Y += delta.Y;
            }

            //Tekend tijdelijk de lijn voor waar de muis nu is
            Point p = eventArgs.GetPosition(Editor);
            if (Enabled && _origin != null) RenderDistance(_origin, _secondPoint ?? new Position((int) p.X, (int) p.Y));

            if (PlexiEnabled && _origin != null)
                RenderPlexiglass(_origin, _secondPoint ?? new Position((int) p.X, (int) p.Y));
        }

        /**
         * Verwijderd alle corona lijnen tussen deze plaatsing en alle andere
         */
        public void RemoveCorona(ProductPlacement removed) {
            if (removed == null) return;
            if (!_lines.ContainsKey(removed)) return;

            //Gaat door alle lijnen heen waar die ooit gemaakt zijn
            foreach (KeyValuePair<ProductPlacement, DistanceLine> entry in _lines[removed]) {
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
        public void CheckCorona(ProductPlacement changed, ProductPlacement skip = null) {
            if (changed?.GetPoly() == null) return;
            if (!_lines.ContainsKey(changed)) _lines[changed] = new Dictionary<ProductPlacement, DistanceLine>();

            //Voegd route toe aan de placements zodat deze ook gecontrolleerd wordt
            List<ProductPlacement> toCheck = ProductPlacements.ToList();
            if (_route != null && _route.Count >= 2) toCheck.Add(_fakeRoute);

            //Gaat door alle producten heen behalve zichzelf en skip
            foreach (ProductPlacement placement in toCheck) {
                if (Equals(placement, changed) || skip != null && Equals(placement, skip) ||
                    placement.GetPoly() == null) continue;

                //Controlleerd door middel van snelle minder accuraten functies hoe het nodig is om te checken
                (bool needed, bool safe) = placement.GetPoly().PreciseNeeded(changed.GetPoly(), 150);
                if (!needed && safe) continue;

                //Kijkt accuraat wat de afstand is
                (Position p1, Position p2) = placement.GetPoly().MinDistance(changed.GetPoly());
                Line lin = new Line(p1, p2);
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
                if (p1.Distance(p2) >= 150 || plexiCheck) {
                    line.Remove(Editor);
                } else {
                    line.P1 = p1;
                    line.P2 = p2;

                    if (!line.Shows) line.Add(Editor);
                }
            }
        }

        public void SetDesign(Design design) {
            // Haalt het design uit de database
            Design = design;
            ProductPlacements ??= new List<ProductPlacement>();
            RoomPlacements = design.Room.RoomPlacements;
            ProductPlacements ??= new List<ProductPlacement>();
            _productOverview = new Dictionary<Product, ProductData>();

            foreach (ProductPlacement placement in ProductPlacements) AddToOverview(placement.Product);
            //Wanneer niet in test env render die de ruimte
            if (Editor != null) {
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

        public void PlaceProduct(Product product, int x, int y) {
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

        public void TryToMoveProduct(ProductPlacement placement, int newX, int newY) {
            //Verwijderd de corona lijnen van de preview
            RemoveCorona(_tempPlacement);
            //Alleen als een object naar het nieuwe punt verplaatst mag worden, wordt het vervangen.
            if (!AllowDrop) {
                //Tekend de corona lijnen van de orginele plaatsin
                CheckCorona(placement);
                return;
            }

            //Verwijder de placement van de placement om te voorkomen dat het product verdubbeld wordt
            int index = ProductPlacements.FindIndex(element => Equals(element, placement));
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

        public void CanvasMouseDown(object sender, MouseButtonEventArgs e) {
            //Rechtermuisknop zorgt ervoor dat informatie over het product wordt getoond
            if (e.ChangedButton == MouseButton.Right) {
                _initialMousePosition = e.GetPosition(Editor);

                //Kaapt het event als er op een hoekpunt van de route is geklikt
                if (RouteEnabled && sender is Ellipse ellipse) {
                    int pos = int.Parse(ellipse.Uid);

                    //Verwijderd dit hoekpunt
                    List<Position> positions = _route.ToList();
                    positions.RemoveAt(pos);
                    _route = new Polygon(positions);
                    RenderRoute();

                    return;
                }

                if (sender is System.Windows.Shapes.Line) {
                    System.Windows.Shapes.Line line = (System.Windows.Shapes.Line) sender;
                    Position p1 = new Position((int) line.X1, (int) line.Y1);
                    Position p2 = new Position((int) line.X2, (int) line.Y2);

                    int index = DistancePlexiLines.FindIndex(i => i.P1.Equals(p1) && i.P2.Equals(p2));

                    if (index == -1) return;

                    PlexiLines.RemoveAt(index);
                    UpdateDbPlexiglass();

                    RenderPolyPlexi();
                    //Editor.Children.Remove();
                }

                if (sender.GetType() == typeof(Canvas)) {
                    _selectedPlacement = null;
                    RenderRoom();
                }

                if (sender.GetType() != typeof(Image)) return;
                Image image = sender as Image;
                IEnumerable<ProductPlacement> placement = ProductPlacements.Where(
                    placement =>
                        placement.X == Canvas.GetLeft(image) && placement.Y == Canvas.GetTop(image)
                );
                if (placement.Count() > 0) _selectedPlacement = placement.First();

                RenderRoom();
            }
            //Linkermuisknop betekend dat het product wordt verplaatst
            else {
                //Als meetlat aanstaat vervangt die deze behavivoer
                if (Enabled) {
                    PlacePoint(e);
                    return;
                }

                //Kaapt het event als looproute aan staat
                if (RouteEnabled) {
                    if (sender is Ellipse)
                        //Verplaatst het hoekpunt waar op geklikt is
                        DragDrop.DoDragDrop(Editor, sender, DragDropEffects.Move);
                    else
                        //Plaats een hoekpunt op de plek waar geklikt is
                        PlaceRoutePoint(e);

                    return;
                }

                if (PlexiEnabled) {
                    PlacePointPlexi(e);
                    return;
                }


                if (sender.GetType() != typeof(Image)) return;
                Image image = sender as Image;
                IEnumerable<ProductPlacement> placement = ProductPlacements.Where(
                    placement =>
                        placement.X == Canvas.GetLeft(image) && placement.Y == Canvas.GetTop(image)
                );
                if (placement.Count() > 0) {
                    _draggingPlacement = placement.First();
                    DragDrop.DoDragDrop(Editor, _draggingPlacement, DragDropEffects.Move);
                }
            }

            // Toegevoegd zodat de corona score wordt bijgewerkt
            // TODO: kan mogelijk beter
            OnPropertyChanged();
        }

        public void CatalogusMouseDown(object sender, MouseButtonEventArgs e) {
            // Linker muisknop moet ingdrukt zijn
            if (e.LeftButton == MouseButtonState.Pressed) {
                if (sender.GetType() != typeof(Image)) return;
                // Cast datacontext naar product
                Product obj = (Product) ((Image) sender).DataContext;
                // Init drag & drop voor geselecteerde product
                DragDrop.DoDragDrop(Editor, obj, DragDropEffects.Link);
            }
        }

        public void CanvasDragDrop(object sender, DragEventArgs e) {
            //Als er geen product is geselecteerd, doe niks
            if (e.Data == null) return;
            if (e.Data.GetDataPresent(typeof(Ellipse))) {
                //Als er een hoek punt van een route wordt versleept kijkt die of deze in de ruimte zit en zo ja past die hem aan
                Position position = new Position((int) e.GetPosition(Editor).X, (int) e.GetPosition(Editor).Y);
                if (!Design.Room.GetPoly().Inside(position)) return;
                int pos = int.Parse(((Ellipse) e.Data.GetData(typeof(Ellipse))).Uid);
                List<Position> positions = _route.ToList();
                positions[pos] = position;
                _route = new Polygon(positions);
                RenderRoute();
                return;
            }

            //In dit geval wordt er een product toegevoegd
            if (e.Data.GetDataPresent(typeof(Product))) {
                Product selectedProduct = (Product) e.Data.GetData(typeof(Product));
                Point position = e.GetPosition(Editor);
                //Trek de helft van de hoogte en breedte van het product eraf
                //Zodat het product in het midden van de cursor staat
                PlaceProduct(
                    selectedProduct,
                    (int) (position.X - selectedProduct.Width / 2),
                    (int) (position.Y - selectedProduct.Length / 2)
                );
                RenderRoom();
            }

            //Hier wordt een product dat al in het design zit verplaatst
            else if (e.Data.GetDataPresent(typeof(ProductPlacement))) {
                ProductPlacement placement = (ProductPlacement) e.Data.GetData(typeof(ProductPlacement));
                Point position = e.GetPosition(Editor);
                int x = (int) position.X - placement.GetPoly().Width / 2;
                int y = (int) position.Y - placement.GetPoly().Length / 2;

                TryToMoveProduct(placement, x, y);
                _draggingPlacement = null;
                RenderRoom();
            }
        }

        public void CanvasDragOver(object sender, DragEventArgs e) {
            //Controleer of er een product is geselecteerd
            if (e.Data == null) return;
            if (e.Data.GetDataPresent(typeof(Ellipse))) {
                CanvasDragDrop(sender, e);
                return;
            }

            Product selectedProduct = null;
            ProductPlacement skip = null;
            int rotation = 0;
            //Afhankelijk van het type data wordt de product op een andere manier opgehaald

            //Haal de positie van de cursor op
            Point position = e.GetPosition(Editor);

            if (e.Data.GetDataPresent(typeof(Product))) {
                selectedProduct = (Product) e.Data.GetData(typeof(Product));
            } else if (e.Data.GetDataPresent(typeof(ProductPlacement))) {
                ProductPlacement placement = (ProductPlacement) e.Data.GetData(typeof(ProductPlacement));
                skip = placement;
                selectedProduct = placement?.Product;
                rotation = placement?.Rotation ?? 0;
            }

            //Als de muis niet bewogen is hoeft het niet opnieuw getekend te worden
            if (position == _previousPosition) return;
            _previousPosition = position;

            _tempPlacement.Product = selectedProduct;
            _tempPlacement.Rotation = rotation;
            _tempPlacement.X = (int) position.X - _tempPlacement.GetPoly().Width / 2;
            _tempPlacement.Y = (int) position.Y - _tempPlacement.GetPoly().Length / 2;

            //Checkt alleen voor afstand bij de nodige producten
            RemoveCorona(skip);
            CheckCorona(_tempPlacement, skip);

            // Check of het product in de ruimte wordt geplaatst
            AllowDrop = CheckRoomCollisions(position, selectedProduct) &&
                        CheckProductCollisions(_tempPlacement);

            //Teken de ruimte en de al geplaatste producten
            // RenderRoom();
            // Render het plaatje vna het product als de cursor binnen de polygon zit
            if (_prevPlace != null) {
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

        /**
         * Rendert alle element in de ruimte opnieuw
         * <br />
         * <b>Dit is erg intensief met veel producten</b>
         */
        private void RenderRoom() {
            //Verwijder alle oude element op het canvas
            if (_screen != null) {
                Editor.Children.Remove(_screen);
                _screen = null;
            }

            foreach (Image image in _images.Values) Editor.Children.Remove(image);

            foreach (Rectangle rect in _rectangles.Values) Editor.Children.Remove(rect);

            _images.Clear();
            _rectangles.Clear();

            for (int i = 0; i < ProductPlacements.Count; i++) {
                ProductPlacement placement = ProductPlacements[i];
                //Controleer of de placement op dat moment verplaatst wordt
                //Als dit het geval is moet de placement doorzichtig worden
                DrawProduct(
                    placement, i, _draggingPlacement == placement, placement.Rotation
                );
            }

            if (_selectedPlacement != null) DrawSelectionButtons(_selectedPlacement);
        }

        private void DrawSelectionButtons(ProductPlacement placement) {
            PlacementSelectScreen selectScreen = new PlacementSelectScreen();
            _screen = selectScreen;
            selectScreen.DataContext = placement.Product;
            // Verwijderd de plaatsing en rendert de ruimte opnieuw
            selectScreen.DeleteButton.Click += delegate {
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
            selectScreen.CloseButton.Click += delegate {
                _selectedPlacement = null;
                Editor.Children.Remove(selectScreen);
            };
            // Roteert het product naar links
            selectScreen.RotateLeftButton.Click += delegate {
                placement.Rotation = placement.Rotation == 0 ? 270 : placement.Rotation -= 90;
                RenderRoom();
            };
            // Roteert het product naar rechts
            selectScreen.RotateRightButton.Click += delegate {
                placement.Rotation = placement.Rotation == 270 ? 0 : placement.Rotation += 90;
                RenderRoom();
            };
            Canvas.SetTop(selectScreen, placement.Y + placement.GetPoly().Length);
            Canvas.SetLeft(selectScreen, placement.X);
            Panel.SetZIndex(selectScreen, 300);
            Editor.Children.Add(selectScreen);
        }

        public (Image i, Rectangle r) DrawProduct(
            ProductPlacement placement,
            int? placementIndex = null,
            bool transparent = false,
            int rotation = 0
        ) {
            //Haal de bestandsnaam van de foto op of gebruik de default
            Product product = placement.Product;
            int x = placement.X;
            int y = placement.Y;

            string? photo = product.Photo ?? "placeholder.png";
            int actualWidth = rotation % 180 == 0 ? product.Width : product.Length;
            int actualLength = rotation % 180 == 0 ? product.Length : product.Width;
            // Veranderd de rotatie van het product
            TransformedBitmap tempBitmap = new TransformedBitmap();

            tempBitmap.BeginInit();
            BitmapImage source = new BitmapImage(new Uri(Environment.CurrentDirectory + $"/Resources/Images/{photo}"));
            tempBitmap.Source = source;
            RotateTransform transform = new RotateTransform(rotation, source.Width / 2, source.Height / 2);
            tempBitmap.Transform = transform;
            tempBitmap.EndInit();

            Image image = new Image {
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

            Rectangle rect = new Rectangle {
                Stroke = Brushes.Red,
                Height = actualLength,
                Width = actualWidth
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

        public void AddToOverview(Product product) {
            double price = product.Price ?? 0.0;
            if (_productOverview.ContainsKey(product)) {
                _productOverview[product].Total = _productOverview[product].Total + 1;
                _productOverview[product].TotalPrice = Math.Round(_productOverview[product].TotalPrice + price, 2);
            } else {
                _productOverview.Add(product, new ProductData {Total = 1, TotalPrice = price});
            }
        }

        public void SetRoomDimensions() {
            List<Position> coordinates = Room.ToList(Design.Room.Positions);

            PointCollection points = new PointCollection();
            // Voeg de punten toe aan een punten collectie
            for (int i = 0; i < coordinates.Count; i++) {
                points.Add(new Point(coordinates[i].X, coordinates[i].Y));
                DistanceLine line = new DistanceLine(coordinates[i], coordinates[(i + 1) % coordinates.Count]);
                line.Add(Editor);
            }

            ImageBrush imgBrush = new ImageBrush();
            imgBrush.ImageSource =
                new BitmapImage(new Uri("../../../Resources/Images/Assets/BluePrint.png", UriKind.Relative));

            RoomPoly.Stroke = Brushes.Black;
            //RoomPoly.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#4A6de5"));
            RoomPoly.Fill = imgBrush;
            RoomPoly.StrokeThickness = 1;
            RoomPoly.HorizontalAlignment = HorizontalAlignment.Left;
            RoomPoly.VerticalAlignment = VerticalAlignment.Center;
            RoomPoly.Points = points;
            Editor.Children.Add(RoomPoly);
        }

        public bool CheckRoomCollisions(Point point, Product product) {
            int yOffset = product.Length / 2;
            int xOffset = product.Width / 2;

            Polygon p = product.GetPoly().Offset((int) point.X - xOffset, (int) point.Y - yOffset);

            foreach (RoomPlacement placement in Design.Room.RoomPlacements)
                if (placement.GetPoly().DoesCollide(p))
                    return false;

            return Design.Room.GetPoly()
                .Inside(p);
        }

        public bool CheckProductCollisions(ProductPlacement placement) {
            Polygon poly = placement.GetPoly();
            foreach (ProductPlacement p in ProductPlacements) {
                if (Equals(p, _draggingPlacement)) continue;
                if (p.GetPoly().DoesCollide(poly)) return false;
            }

            return true;
        }

        public void SetRoomScale() {
            double scale;

            // Zet de dimensies van de ruimte polygon
            RoomPoly.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            // Als de breedte hoger is dan de breedte wordt de breedte gebruikt voor de schaal en vice versa
            if (RoomPoly.DesiredSize.Width > RoomPoly.DesiredSize.Height)
                scale = (Navigator.Instance.CurrentPage.ActualWidth - 260) / RoomPoly.DesiredSize.Width;
            else scale = (Navigator.Instance.CurrentPage.ActualHeight - 20) / RoomPoly.DesiredSize.Height;

            ScaleCanvas(scale, _initialMousePosition);
        }

        public void CanvasMouseScroll(object sender, MouseWheelEventArgs e) {
            Point mousePosition = e.GetPosition(Editor);

            double scaleFactor = 1.05;
            if (e.Delta < 0) scaleFactor = 1 / scaleFactor;

            ScaleCanvas(scaleFactor, mousePosition);
        }

        private void ScaleCanvas(double scale, Point position) {
            // Kijkt of de gegeven schaal binnen de pagina past, zo niet veranderd de schaal niet
            zoomTransform.CenterX = position.X;
            zoomTransform.CenterY = position.Y;

            zoomTransform.ScaleX *= scale;
            zoomTransform.ScaleY *= scale;

            Point
                cursorpos = Mouse.GetPosition(
                    Editor
                ); //This was the secret, as the mouse position gets out of whack when the transform occurs, but Mouse.GetPosition lets us get the point accurate to the transformed canvas.

            double discrepancyX = cursorpos.X - position.X;
            double discrepancyY = cursorpos.Y - position.Y;

            //If your canvas is already panned an arbitrary amount, this aggregates the discrepancy to the TranslateTransform.
            panTransform.X += discrepancyX;
            panTransform.Y += discrepancyY;
        }

        internal void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static string FromList(List<DistanceLine> distancelines) {
            string returnstring = "";
            if (distancelines == null) return null;
            //IEnumerable<Position> enumerable = positions.ToList();
            foreach (DistanceLine position in distancelines)
                returnstring = $"{returnstring}{position.P1};{position.P2}|";

            return returnstring;
        }

        public static List<DistanceLine> ToList(string DistanceLines) {
            switch (DistanceLines) {
                case null:
                case "": {
                    return new List<DistanceLine>();
                }
                default: {
                    List<DistanceLine> list = new List<DistanceLine>();
                    List<string> lines = DistanceLines.Split("|").ToList();
                    foreach (string line in lines) {
                        if (line == "") continue;
                        List<Position> positions = line.Split(";")
                            .Select(p => p.Split(",").Select(int.Parse).ToList())
                            .Select(p => new Position(p[0], p[1]))
                            .ToList();
                        list.Add(new DistanceLine(positions[0], positions[1]));
                    }

                    return list;
                }
            }
        }
    }

    public class ProductData {
        public int Total { get; set; }
        public double TotalPrice { get; set; }
    }

    //Holder for a polygon as placement, so this works with the distance check method
    internal class FakePlacement : ProductPlacement {
        public Polygon Poly { get; set; }

        public override Polygon GetPoly() { return Poly; }
    }
}