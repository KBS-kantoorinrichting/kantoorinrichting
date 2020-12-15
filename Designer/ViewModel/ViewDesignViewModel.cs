using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Designer.ViewModel {
    public class ViewDesignViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<Product> Products { get; set; }
        private Dictionary<Product, ProductData> _productOverview { get; set; }

        public List<KeyValuePair<Product, ProductData>> ProductOverview => _productOverview.ToList();
        public double TotalPrice => _productOverview.Sum(p => p.Value.TotalPrice);
        public List<ProductPlacement> ProductPlacements { get; set; }
        public ArgumentCommand<DragEventArgs> DragDropCommand { get; set; }
        public ArgumentCommand<DragEventArgs> DragOverCommand { get; set; }
        public ArgumentCommand<MouseButtonEventArgs> CatalogusMouseDownCommand { get; set; }
        public ArgumentCommand<MouseButtonEventArgs> CanvasMouseDownCommand { get; set; }
        public ArgumentCommand<MouseEventArgs> MouseMoveCommand { get; set; }
        public BasicCommand Measure { get; set; }
        public BasicCommand Layout { get; set; }
        public BasicCommand ClearProducts { get; set; }
        public BasicCommand RemoveRoute { get; set; }
        public ArgumentCommand<MouseWheelEventArgs> CanvasMouseScrollCommand { get; set; }
        public Product SelectedProduct => _selectedPlacement.Product;
        public Design Design { get; set; }
        public Canvas Editor { get; set; }
        private Point _previousPosition;
        private ProductPlacement _selectedPlacement;
        private ProductPlacement _draggingPlacement;
        public Polygon RoomPoly { get; set; }
        public bool AllowDrop = false;

        public int DistanceScore {
            get {
                int increment = 0;
                if (ProductPlacements == null) return 0;

                List<ProductPlacement> placements = ProductPlacements.ToList();

                //Loopt door alle paren van producten zonder overbodige stappen zoals p1 -> p1 en p1 -> p2, p2 -> p1
                for (int i = 0; i < placements.Count; i++) {
                    bool noDistance = false;
                    ProductPlacement placement1 = placements[i];
                    for (int j = 0; j < placements.Count; j++) {
                        ProductPlacement placement2 = placements[j];
                        if (j != i) {
                            (Position p1, Position p2) = placement1.GetPoly().MinDistance(placement2.GetPoly());

                            double distance = p1.Distance(p2);
                            if (!noDistance) noDistance = distance <= 150;
                        }
                    }

                    if (noDistance) increment++;
                }

                double reversedIncrement = ProductPlacements.Count - increment;

                return (int) (reversedIncrement / ProductPlacements.Count * 100);
            }
        }

        public int VentilationScore { get; set; } = 80;
        public int RouteScore { get; set; } = 20;
        public double Scale = 1.0;
        private double _canvasHeight => Navigator.Instance.CurrentPage.ActualHeight - 20;

        private double _canvasWidth => Navigator.Instance.CurrentPage.ActualWidth - 260;
        private readonly MatrixTransform _transform = new MatrixTransform();
        private Point _initialMousePosition;

        //Special constructor for unit tests
        public ViewDesignViewModel(Design design) {
            SetDesign(design);
            Products = LoadProducts();
        }

        public ViewDesignViewModel() {
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
            Layout = new BasicCommand(GenerateLayout);
            RemoveRoute = new BasicCommand(DeleteRoute);
            ClearProducts = new BasicCommand(Clear);
            CanvasMouseScrollCommand =
                new ArgumentCommand<MouseWheelEventArgs>(e => CanvasMouseScroll(e.OriginalSource, e));
            _productOverview = new Dictionary<Product, ProductData>();

            _distanceLine = new DistanceLine(null, null);
        }

        public bool Enabled { get; set; }

        private Position _origin;
        private Position _secondPoint;
        private DistanceLine _distanceLine;

        //Verwijder de lijn als er al een meetlat was
        public void StartMeasure() {
            if (!Enabled) return;
            _distanceLine.Remove(Editor);
            _origin = null;
            _secondPoint = null;
        }

        public bool RouteEnabled { get; set; }

        private Models.Polygon _route {
            get => Design.GetRoutePoly();
            set => Design.Route = value?.Convert();
        }

        /**
         * Verwijderd de route
         */
        public void DeleteRoute() {
            _route = null;

            int distance = 50;

            List<Models.Line> lines = Design.Room.GetPoly().GetLines().ToList();
            List<Models.Line> correct = lines.Select(l => (Models.Line) null).ToList();
            int start = -1;
            for (int i = 0; i < lines.Count; i++) {
                Models.Line l1 = lines[i];
                Models.Line l2 = lines[(i + 1) % lines.Count];

                Models.Line foundL1 = null;
                Models.Line foundL2 = null;
                for (int r = 0; r < 4; r++) {
                    Models.Line tempL1 = l1.OffsetPerpendicular(distance, r % 2 == 0);
                    Models.Line tempL2 = l2.OffsetPerpendicular(distance, r / 2 == 0);

                    Position inter = tempL1.Intersection(tempL2);
                    if (inter == null || !Design.Room.GetPoly().Inside(inter)) continue;
                    if (foundL1 != null) {
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
            
            
            for (int i = start; i != start - 1; i = (i + 1) % lines.Count) {
                int j = (i + 1) % lines.Count;
                if (correct[j] != null) break;
                Models.Line before = correct[i];
                Models.Line toTest = lines[j];
            
                Models.Line l1 = toTest.OffsetPerpendicular(distance, true);
                Models.Line l2 = toTest.OffsetPerpendicular(distance, false);
            
                Position inter1 = before.Intersection(l1);
                if (inter1 == null || !Design.Room.GetPoly().Inside(inter1)) {
                    correct[j] = l2;
                    continue;
                }
                Position inter2 = before.Intersection(l2);
                if (inter2 == null || !Design.Room.GetPoly().Inside(inter2)) {
                    correct[j] = l1;
                    continue;
                }
            
                double d1 = before.P1.Distance(inter1);
                double d2 = before.P1.Distance(inter2);
            
                correct[j] = d1 > d2 ? l1 : l2;
            }

            List<Position> positions = new List<Position>(); 
            for (int i = 0; i < correct.Count; i++) {
                Models.Line l1 = correct[i];
                Models.Line l2 = correct[(i + 1) % lines.Count];
                positions.Add(l1.Intersection(l2));
            }

            positions.RemoveAll(p => p == null);

            _route = new Models.Polygon(positions);

            ShowRoute();
        }

        List<DistanceLine> _routeLines = new List<DistanceLine>();
        List<Ellipse> _ellipses = new List<Ellipse>();

        /**
         * Tekent de volledige route
         */
        public void ShowRoute() {
            //Verwijderd eerst de volledige lijn
            _routeLines.ForEach(l => l.Remove(Editor));
            _routeLines.Clear();
            _ellipses.ForEach(Editor.Children.Remove);
            _ellipses.Clear();
            if (_route == null || _route.Count == 0) return;
            //Tekend de volledige lijn
            foreach (Models.Line line in _route.GetLines()) {
                _routeLines.Add(new DistanceLine(line.P1, line.P2));
            }

            //Tekend alle hoek punten
            for (int index = 0; index < _route.Count; index++) {
                Position position = _route[index];
                int size = 10;
                Ellipse ellipse = new Ellipse() {
                    Height = size,
                    Width = size,
                    Fill = Brushes.Black,
                    Uid = index.ToString(),
                };

                _ellipses.Add(ellipse);
                Editor.Children.Add(ellipse);
                Canvas.SetLeft(ellipse, position.X - size / 2);
                Canvas.SetTop(ellipse, position.Y - size / 2);
                Canvas.SetZIndex(ellipse, 300);
            }

            _routeLines.ForEach(l => l.Add(Editor));

            //Controlleerd alle plaatsing tegen over de route
            _fakeRoute.poly = _route;
            CheckCorona(_fakeRoute);
        }

        /**
         * Verwijderd alle plaatsingen
         */
        public void Clear() {
            ProductPlacements.ForEach(RemoveCorona);
            ProductPlacements.Clear();

            RenderRoom();
        }

        /**
         * Loopt alle mogelijke plaatsingen door om vervolgens de kamer zo vol mogelijk te krijgen
         */
        public void GenerateLayout() {
            Models.Polygon room = Design.Room.GetPoly();
            //TODO Pakt momententeel nog het eerste product om te plaatsen
            Product product = Products.First();

            //Pakt de minimale en maximale punten
            Position min = room.Min();
            Position max = room.Max();

            //Berekend de afstand per stap door middel van de afstand tussen de 2 boudning hoeken punten,
            //als dit te groot wordt pakt die de product lengte of hoogte ligt er aan welke kleiner is 
            int accuracy = Math.Min((int) min.Distance(max) / 200, Math.Min(product.Length, product.Width));

            //Maakt een nieuwe thread aan waar de dingen gecontrolleerd worden zodat je live de producten ziet plaatsen
            new Thread(
                () => {
                    //Loopt door alle coordinaten binnen de ruimte boudning box heen met stappen van accuracy
                    for (int y = min.Y + 1; y < max.Y; y += accuracy) {
                        for (int x = min.X + 1; x < max.X; x += accuracy) {
                            Position position = new Position(x, y);

                            //Als het punt binnen de ruimte zit controlleerd die of deze genoeg afstand heeft van alles
                            if (room.Inside(product.GetPoly().Offset(position))) {
                                ProductPlacement placement = new ProductPlacement(position, product, null);
                                bool success = true;
                                //Loopt alle plaatsingen langs om te kijken of die veilig is om te plaatsen
                                for (int i = 0; i < ProductPlacements.Count; i++) {
                                    ProductPlacement place = ProductPlacements[i];
                                    if (place.GetPoly().IsSafe(placement.GetPoly())) continue;

                                    success = false;
                                    break;
                                }

                                //Kijkt of die ver genoeg van de lijn is
                                if (success && _route != null && _route.Count >= 2) {
                                    (Position p1, Position p2) = placement.GetPoly().MinDistance(_route);
                                    if (p1.Distance(p2) < 150) success = false;
                                }

                                //Als dit allemaal klopt voegd die het product toe;
                                if (success) {
                                    ProductPlacements.Add(placement);

                                    AddToOverview(placement.Product);

                                    Editor.Dispatcher.Invoke(
                                        () => {
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

        /**
         * Plaatst een nieuwe hoek punt voor de route
         */
        private void PlaceRoutePoint(MouseButtonEventArgs eventArgs) {
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
            _route = new Models.Polygon(positions);
            ShowRoute();
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

        public void HandleMouseMove(MouseEventArgs eventArgs) {
            if (eventArgs.RightButton == MouseButtonState.Pressed) {
                Point mousePosition = eventArgs.GetPosition(Editor);
                Vector delta = Point.Subtract(mousePosition, _initialMousePosition);
                var translate = new TranslateTransform(delta.X, delta.Y);
                _transform.Matrix = translate.Value * _transform.Matrix;

                Editor.RenderTransform = _transform;
            }

            //Wanneer meetlat niet aan staat of er geen begin punt is van de lijn stopt die
            if (!Enabled || _origin == null) return;

            //Tekend tijdelijk de lijn voor waar de muis nu is
            Point p = eventArgs.GetPosition(Editor);
            RenderDistance(_origin, _secondPoint ?? new Position((int) p.X, (int) p.Y));
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
            if (!_lines.ContainsKey(changed)) {
                _lines[changed] = new Dictionary<ProductPlacement, DistanceLine>();
            }

            //Voegd route toe aan de placements zodat deze ook gecontrolleerd wordt
            List<ProductPlacement> toCheck = new List<ProductPlacement>(ProductPlacements);
            if (_route != null && _route.Count >= 2) toCheck.Add(_fakeRoute);

            //Gaat door alle producten heen behalve zichzelf en skip
            foreach (ProductPlacement placement in toCheck) {
                if (Equals(placement, changed) || (skip != null && Equals(placement, skip)) ||
                    placement.GetPoly() == null) continue;

                //Controlleerd door middel van snelle minder accuraten functies hoe het nodig is om te checken
                (bool needed, bool safe) = placement.GetPoly().PreciseNeeded(changed.GetPoly(), 150);
                if (!needed && safe) continue;

                //Kijkt accuraat wat de afstand is
                (Position p1, Position p2) = placement.GetPoly().MinDistance(changed.GetPoly());

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
                if (p1.Distance(p2) >= 150) {
                    line.Remove(Editor);
                } else {
                    line.P1 = p1;
                    line.P2 = p2;

                    if (!line.Shows) line.Add(Editor);
                }
            }
        }

        public void SetDesign(Design design) {
            Design = design;
            ProductPlacements = design.ProductPlacements;
            ProductPlacements ??= new List<ProductPlacement>();
            _productOverview = new Dictionary<Product, ProductData>();
            //Wanneer niet in test env render die de ruimte
            if (Editor != null) {
                // Sets the dimensions of the current room
                SetRoomDimensions();
                RenderRoom();

                //Tekend de route en alle corona lijnen
                ProductPlacements.ForEach(p => CheckCorona(p));
                ShowRoute();

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
                //Tekent de corona lijnen van de orginele plaatsin
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
                    _route = new Models.Polygon(positions);
                    ShowRoute();

                    return;
                }

                if (sender.GetType() == typeof(Canvas)) {
                    _selectedPlacement = null;
                    RenderRoom();
                }

                if (sender.GetType() != typeof(Image)) return;
                var image = sender as Image;
                var placement = ProductPlacements.Where(
                    placement =>
                        placement.X == Canvas.GetLeft(image) && placement.Y == Canvas.GetTop(image)
                );
                if (placement.Count() > 0) {
                    _selectedPlacement = placement.First();
                }

                RenderRoom();
            }
            //Linkermuisknop betekent dat het product wordt verplaatst
            else {
                //Als meetlat aanstaat vervangt die deze behavivoer
                if (Enabled) {
                    PlacePoint(e);
                    return;
                }

                //Kaapt het event als looproute aan staat
                if (RouteEnabled) {
                    if (sender is Ellipse) {
                        //Verplaatst het hoekpunt waar op geklikt is
                        DragDrop.DoDragDrop(Editor, sender, DragDropEffects.Move);
                    } else {
                        //Plaats een hoekpunt op de plek waar geklikt is
                        PlaceRoutePoint(e);
                    }

                    return;
                }

                if (sender.GetType() != typeof(Image)) return;
                var image = sender as Image;
                var placement = ProductPlacements.Where(
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
                var obj = (Product) ((Image) sender).DataContext;
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
                _route = new Models.Polygon(positions);
                ShowRoute();
                return;
            }

            //In dit geval wordt er een product toegevoegd
            if (e.Data.GetDataPresent(typeof(Product))) {
                var selectedProduct = (Product) e.Data.GetData(typeof(Product));
                Point position = e.GetPosition(Editor);
                //Trek de helft van de hoogte en breedte van het product eraf
                //Zodat het product in het midden van de cursor staat
                PlaceProduct(
                    selectedProduct,
                    (int) (position.X - (selectedProduct.Width / 2)),
                    (int) (position.Y - (selectedProduct.Length / 2))
                );
                RenderRoom();
            }

            //Hier wordt een product dat al in het design zit verplaatst
            else if (e.Data.GetDataPresent(typeof(ProductPlacement))) {
                ProductPlacement placement = (ProductPlacement) e.Data.GetData(typeof(ProductPlacement));
                Point position = e.GetPosition(Editor);
                int x = (int) position.X - (placement.GetPoly().Width / 2);
                int y = (int) position.Y - (placement.GetPoly().Length / 2);

                TryToMoveProduct(placement, x, y);
                _draggingPlacement = null;
                RenderRoom();
            }
        }

        private readonly ProductPlacement _tempPlacement = new ProductPlacement();

        private (Image i, Rectangle r)? _prevPlace;

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

        private PlacementSelectScreen _screen;

        /**
         * Rendert alle element in de ruimte opnieuw<br/>
         * <b>Dit is erg intensief met veel producten</b>
         */
        private void RenderRoom() {
            //Verwijder alle oude element op het canvas
            if (_screen != null) {
                Editor.Children.Remove(_screen);
                _screen = null;
            }

            foreach (Image image in _images.Values) {
                Editor.Children.Remove(image);
            }

            foreach (Rectangle rect in _rectangles.Values) {
                Editor.Children.Remove(rect);
            }

            _images.Clear();
            _rectangles.Clear();

            for (int i = 0; i < ProductPlacements.Count; i++) {
                var placement = ProductPlacements[i];
                //Controleer of de placement op dat moment verplaatst wordt
                //Als dit het geval is moet de placement doorzichtig worden
                DrawProduct(
                    placement, i, _draggingPlacement == placement, placement.Rotation
                );
            }

            if (_selectedPlacement != null) {
                DrawSelectionButtons(_selectedPlacement);
            }
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

        public Dictionary<ProductPlacement, Image> _images = new Dictionary<ProductPlacement, Image>();
        public Dictionary<ProductPlacement, Rectangle> _rectangles = new Dictionary<ProductPlacement, Rectangle>();

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

            var image = new Image {
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

            var rect = new Rectangle() {
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

        public void AddToOverview(Product product) {
            var price = product.Price ?? 0.0;
            if (_productOverview.ContainsKey(product)) {
                _productOverview[product].Total = _productOverview[product].Total + 1;
                _productOverview[product].TotalPrice = Math.Round(_productOverview[product].TotalPrice + price, 2);
            } else {
                _productOverview.Add(product, new ProductData() {Total = 1, TotalPrice = price});
            }
        }

        public void SetRoomDimensions() {
            var coordinates = Room.ToList(Design.Room.Positions);

            PointCollection points = new PointCollection();
            // Voeg de punten toe aan een punten collectie
            for (int i = 0; i < coordinates.Count; i++) {
                points.Add(new Point(coordinates[i].X, coordinates[i].Y));
                DistanceLine line = new DistanceLine(coordinates[i], coordinates[(i + 1) % coordinates.Count]);
                line.Add(Editor);
            }

            RoomPoly.Stroke = Brushes.Black;
            RoomPoly.Fill = Brushes.LightGray;
            RoomPoly.StrokeThickness = 1;
            RoomPoly.HorizontalAlignment = HorizontalAlignment.Left;
            RoomPoly.VerticalAlignment = VerticalAlignment.Center;
            RoomPoly.Points = points;
            Editor.Children.Add(RoomPoly);
        }

        public bool CheckRoomCollisions(Point point, Product product) {
            int yOffset = product.Length / 2;
            int xOffset = product.Width / 2;

            return Design.Room.GetPoly()
                .Inside(product.GetPoly().Offset((int) point.X - xOffset, (int) point.Y - yOffset));
        }

        public bool CheckProductCollisions(ProductPlacement placement) {
            Models.Polygon poly = placement.GetPoly();
            foreach (ProductPlacement p in ProductPlacements) {
                if (Equals(p, _draggingPlacement)) continue;
                if (p.GetPoly().DoesCollide(poly)) {
                    return false;
                }
            }

            return true;
        }

        public void SetRoomScale() {
            double scale;

            // Zet de dimensies van de ruimte polygon
            RoomPoly.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            // Als de breedte hoger is dan de breedte wordt de breedte gebruikt voor de schaal en vice versa
            if (RoomPoly.DesiredSize.Width > RoomPoly.DesiredSize.Height) {
                scale = (Navigator.Instance.CurrentPage.ActualWidth - 260) / RoomPoly.DesiredSize.Width;
            } else {
                scale = (Navigator.Instance.CurrentPage.ActualHeight - 20) / RoomPoly.DesiredSize.Height;
            }

            ScaleCanvas(scale, _initialMousePosition);
        }

        public void CanvasMouseScroll(object sender, MouseWheelEventArgs e) {
            Point mousePosition = e.GetPosition(Editor);

            double scaleFactor = 1.05;
            if (e.Delta < 0) {
                scaleFactor = 1 / scaleFactor;
            }

            ScaleCanvas(scaleFactor, mousePosition);
        }

        private void ScaleCanvas(double scale, Point mousePosition) {
            // Kijkt of de gegeven schaal binnen de pagina past, zo niet veranderd de schaal niet
            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(scale, scale, mousePosition.X, mousePosition.Y);
            _transform.Matrix = scaleMatrix;
            Editor.RenderTransform = _transform;
        }

        private void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ProductData {
        public int Total { get; set; }
        public double TotalPrice { get; set; }
    }

    public class DistanceLine {
        private Line _line;
        private Line _line2;
        private TextBlock _textBlock;
        private Position _p1;
        private Position _p2;

        public Position P1 {
            get => _p1;
            set {
                _p1 = value;
                UpdatePositions();
            }
        }

        public Position P2 {
            get => _p2;
            set {
                _p2 = value;
                UpdatePositions();
            }
        }

        public bool Shows { get; private set; }

        public DistanceLine(Position p1, Position p2) {
            _p1 = p1;
            _p2 = p2;
            _line = new Line();
            _line2 = new Line();
            _textBlock = new TextBlock();

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
        public void Add(Canvas editor) {
            Shows = true;
            editor.Children.Add(_line);
            editor.Children.Add(_line2);
            editor.Children.Add(_textBlock);
            Render();
        }

        /**
         * Set stuff that needs the canvas to be used
         */
        private void Render() {
            Panel.SetZIndex(_line, 100);
            Panel.SetZIndex(_line2, 101);
            Panel.SetZIndex(_textBlock, 102);

            UpdatePositions();
        }

        /**
         * Updates the lines so that they reflect P1 and P2
         */
        private void UpdatePositions() {
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
            _textBlock.Text = FormatText(P1.Distance(P2));
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
        private Size MeasureString() {
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
        private static string FormatText(double distance) {
            if (distance < 100) {
                return distance.ToString("F0") + " cm";
            }

            return (distance / 100).ToString("F2") + " m";
        }

        /**
         * Removes the lines and text from the canvas
         */
        public void Remove(Canvas editor) {
            Shows = false;
            editor.Children.Remove(_line);
            editor.Children.Remove(_line2);
            editor.Children.Remove(_textBlock);
        }

        /**
         * Converts radians to degrees
         */
        private static double ConvertRadiansToDegrees(double radians) {
            double degrees = 180 / Math.PI * radians;
            if (degrees > 90) return degrees + 180;
            if (degrees < -90) return degrees + 180;
            return degrees;
        }
    }

    //Holder for a polygon as placement, so this works with the distance check method
    internal class FakePlacement : ProductPlacement {
        public Models.Polygon poly { get; set; }

        public override Models.Polygon GetPoly() => poly;
    }
}