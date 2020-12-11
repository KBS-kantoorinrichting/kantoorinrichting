using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Designer.Other;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Designer.View;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Globalization;
using System.Threading;
using Models;
using Models.Utils;
using Services;
using Polygon = System.Windows.Shapes.Polygon;
using System.Diagnostics;

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
        public ArgumentCommand<MouseWheelEventArgs> CanvasMouseScrollCommand { get; set; }
        public Product SelectedProduct => _selectedPlacement.Product;
        public Design Design { get; set; }
        public Canvas Editor { get; set; }
        private Point _previousPosition;
        private ProductPlacement _selectedPlacement;
        private ProductPlacement _draggingPlacement;
        public Polygon RoomPoly { get; set; }
        public bool AllowDrop = false;
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
            ClearProducts = new BasicCommand(Clear);
            CanvasMouseScrollCommand =
                new ArgumentCommand<MouseWheelEventArgs>(e => CanvasMouseScroll(e.OriginalSource, e));
            _productOverview = new Dictionary<Product, ProductData>();

            _distanceLine = new DistanceLine(null, null);
        }

        private bool _enabled;

        public bool Enabled {
            get => _enabled;
            set => _enabled = value;
        }

        private Position _origin;
        private Position _secondPoint;
        private DistanceLine _distanceLine;

        public void StartMeasure() {
            if (!_enabled) return;
            _distanceLine.Remove(Editor);
            _origin = null;
            _secondPoint = null;
        }

        public void Clear() {
            ProductPlacements.ForEach(RemoveCorona);
            ProductPlacements.Clear();

            RenderRoom();
        }

        public void RenderRoomFrames()
        {
            if(Design.Room.RoomPlacements != null)
            {
                Debug.WriteLine(Design.Room.RoomPlacements.Count);
                foreach (RoomPlacement frame in Design.Room.RoomPlacements)
                {
                    Debug.WriteLine(frame.GetPoly().GetPolygon());
                    Editor.Children.Add(frame.GetPoly().GetPolygon());
                }
            }
        }

        public void GenerateLayout() {
            // ProductPlacements.Clear();

            Models.Polygon room = Design.Room.GetPoly();
            Product product = Products.First();

            Position min = room.Min();
            Position max = room.Max();

            int accuracy = Math.Min((int) min.Distance(max) / 200, Math.Min(product.Length, product.Width));

            new Thread(
                () => {
                    for (int y = min.Y + 1; y < max.Y; y += accuracy) {
                        for (int x = min.X + 1; x < max.X; x += accuracy) {
                            Position position = new Position(x, y);

                            if (room.Inside(product.GetPoly().Offset(position))) {
                                ProductPlacement placement = new ProductPlacement(position, Products.First(), null);
                                bool success = true;
                                for (int i = 0; i < ProductPlacements.Count; i++) {
                                    ProductPlacement place = ProductPlacements[i];
                                    if (place.GetPoly().IsSafe(placement.GetPoly())) continue;

                                    success = false;
                                    break;
                                }

                                if (success) {
                                    ProductPlacements.Add(placement);

                                    AddToOverview(placement.Product);

                                    Editor.Dispatcher.Invoke(RenderRoom);
                                }
                            }
                        }
                    }
                }
            ).Start();
        }

        private void PlacePoint(MouseButtonEventArgs eventArgs) {
            Point p = eventArgs.GetPosition(Editor);

            if (_origin == null || _secondPoint != null) {
                _origin = new Position((int) p.X, (int) p.Y);
                _secondPoint = null;
            } else {
                _secondPoint = new Position((int) p.X, (int) p.Y);
                _enabled = false;
                OnPropertyChanged();
            }
        }

        public void RenderDistance(Position p1, Position p2) {
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

            if (!_enabled || _origin == null) return;

            Point p = eventArgs.GetPosition(Editor);
            RenderDistance(_origin, _secondPoint ?? new Position((int) p.X, (int) p.Y));
        }

        private List<DistanceLine> _coronaLines = new List<DistanceLine>();

        private Dictionary<ProductPlacement, Dictionary<ProductPlacement, DistanceLine>> _lines =
            new Dictionary<ProductPlacement, Dictionary<ProductPlacement, DistanceLine>>();

        public void RemoveCorona(ProductPlacement removed) {
            if (removed == null) return;
            if (!_lines.ContainsKey(removed)) return;

            foreach (KeyValuePair<ProductPlacement, DistanceLine> entry in _lines[removed]) {
                _lines[entry.Key].Remove(removed);
                entry.Value.Remove(Editor);
            }
        }

        public void CheckCorona(ProductPlacement changed, ProductPlacement skip = null) {
            if (changed == null) return;
            if (!_lines.ContainsKey(changed)) {
                _lines[changed] = new Dictionary<ProductPlacement, DistanceLine>();
            }

            for (int i = 0; i < ProductPlacements.Count; i++) {
                ProductPlacement placement = ProductPlacements[i];
                if (Equals(placement, changed) || skip != null && Equals(placement, skip)) continue;
                (bool needed, bool safe) = placement.GetPoly().PreciseNeeded(changed.GetPoly(), 150);
                if (!needed && safe) continue;

                (Position p1, Position p2) = placement.GetPoly().MinDistance(changed.GetPoly());
                DistanceLine line = _lines[changed].ContainsKey(placement)
                    ? _lines[changed][placement]
                    : new DistanceLine(null, null);

                _lines[changed][placement] = line;
                if (!_lines.ContainsKey(placement))
                    _lines[placement] = new Dictionary<ProductPlacement, DistanceLine>();

                _lines[placement][changed] = line;

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

                RenderRoomFrames();

                ProductPlacements.ForEach(p => CheckCorona(p));

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
            if (Editor != null) RenderRoom();
            if (Editor != null) CheckCorona(placement);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void TryToMoveProduct(ProductPlacement placement, int newX, int newY) {
            RemoveCorona(_tempPlacement);
            //Alleen als een object naar het nieuwe punt verplaatst mag worden, wordt het vervangen.
            if (!AllowDrop) {
                CheckCorona(placement);
                return;
            }

            //Verwijder de placement van de placement om te voorkomen dat het product verdubbeld wordt
            var index = ProductPlacements.FindIndex(element => Equals(element, placement));
            //Trek de helft van de hoogte en breedte van het product eraf
            //Zodat het product in het midden van de cursor staat
            placement.X = newX;
            placement.Y = newY;
            RemoveCorona(ProductPlacements[index]);
            //Na het aanpassen wordt het weer toegevoegd om de illusie te geven dat het in de lijst wordt aangepast
            ProductPlacements[index] = placement;
            RenderRoom();
            CheckCorona(placement);
        }

        public void CanvasMouseDown(object sender, MouseButtonEventArgs e) {
            //Rechtermuisknop zorgt ervoor dat informatie over het product wordt getoond
            if (e.ChangedButton == MouseButton.Right) {
                _initialMousePosition = e.GetPosition(Editor);

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
                if (_enabled) {
                    PlacePoint(e);
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

        private ProductPlacement _tempPlacement = new ProductPlacement();

        private (Image i, Rectangle r)? _prevPlace = null;

        public void CanvasDragOver(object sender, DragEventArgs e) {
            //Controleer of er een product is geselecteerd
            if (e.Data == null) return;
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

        private void RenderRoom() {
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
                RemoveCorona(placement);
                Editor.Children.Remove(selectScreen);
                Editor.Children.Remove(_images[placement]);
                Editor.Children.Remove(_rectangles[placement]);
                _images.Remove(placement);
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

            _rectangles[placement] = rect;

            Canvas.SetTop(rect, y);
            Canvas.SetLeft(rect, x);
            Editor.Children.Add(rect);
            // Voegt het id van het productplacement index in de productplacement list
            image.Uid ??= placementIndex.ToString();

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

        private bool _shows = false;
        public bool Shows => _shows;

        public DistanceLine(Position p1, Position p2) {
            _p1 = p1;
            _p2 = p2;
            _line = new Line();
            _line2 = new Line();
            _textBlock = new TextBlock();
        }

        public void Add(Canvas editor) {
            _shows = true;
            editor.Children.Add(_line);
            editor.Children.Add(_line2);
            editor.Children.Add(_textBlock);
            Render();
        }

        public void Render() {
            _line.Stroke = Brushes.White;
            _line.StrokeThickness = 3;

            Panel.SetZIndex(_line, 100);
            _line2.Stroke = Brushes.Black;
            _line2.StrokeThickness = 1;

            Panel.SetZIndex(_line2, 101);
            Panel.SetZIndex(_textBlock, 102);

            _textBlock.Foreground = new SolidColorBrush(Colors.Black);
            _textBlock.Background = new SolidColorBrush(Colors.White);

            UpdatePositions();
        }

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
            double dy = size.Height / 2;

            double radians = Math.Atan2(P2.Y - P1.Y, P2.X - P1.X);
            double degrees = ConvertRadiansToDegrees(radians);
            _textBlock.RenderTransform = new RotateTransform(degrees, dx, 0);

            Canvas.SetLeft(_textBlock, center.X - dx);
            Canvas.SetTop(_textBlock, center.Y - 0);
        }

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

        private string FormatText(double distance) {
            if (distance < 100) {
                return distance.ToString("F0") + " cm";
            } else {
                return (distance / 100).ToString("F2") + " m";
            }
        }

        public void Remove(Canvas editor) {
            _shows = false;
            editor.Children.Remove(_line);
            editor.Children.Remove(_line2);
            editor.Children.Remove(_textBlock);
        }

        private static double ConvertRadiansToDegrees(double radians) {
            double degrees = 180 / Math.PI * radians;
            if (degrees > 90) return degrees + 180;
            if (degrees < -90) return degrees + 180;
            return degrees;
        }
    }
}