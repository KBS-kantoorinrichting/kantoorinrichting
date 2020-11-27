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
using System.Diagnostics;
using System.Globalization;
using Designer.Utils;
using Models;
using Services;

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
        public ArgumentCommand<MouseWheelEventArgs> CanvasMouseScrollCommand { get; set; }
        public ArgumentCommand<SizeChangedEventArgs> ResizeCommand { get; set; }
        public Product SelectedProduct => _selectedPlacement.Product;
        public Design Design { get; set; }
        public Canvas Editor { get; set; }
        private Point _previousPosition;
        private ProductPlacement _selectedPlacement;
        private ProductPlacement _draggingPlacement;
        public Polygon RoomPoly { get; set; }
        public bool AllowDrop = false;
        public double Scale = 1.0;
        private double _canvasHeight => Navigator.Instance.CurrentPage.ActualHeight - 20;

        private double _canvasWidth => Navigator.Instance.CurrentPage.ActualWidth - 260;

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
            CanvasMouseScrollCommand =
                new ArgumentCommand<MouseWheelEventArgs>(e => CanvasMouseScroll(e.OriginalSource, e));
            ResizeCommand = new ArgumentCommand<SizeChangedEventArgs>(e => ResizePage(e.OriginalSource, e));
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
            if (_enabled) {
                _distanceLine.Remove(Editor);
                _origin = null;
                _secondPoint = null;
            }
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
            if (!_enabled || _origin == null) return;

            Point p = eventArgs.GetPosition(Editor);
            RenderDistance(_origin, _secondPoint ?? new Position((int) p.X, (int) p.Y));
        }

        private List<DistanceLine> _coronaLines = new List<DistanceLine>();

        public void CheckCorona(ProductPlacement temp = null, ProductPlacement skip = null) {
            _coronaLines.ForEach(line => line.Remove(Editor));
            _coronaLines.Clear();

            List<ProductPlacement> placements = ProductPlacements.ToList();
            if (temp != null) placements.Add(temp);

            //Loopt door alle paren van producten zonder overbodige stappen zoals p1 -> p1 en p1 -> p2, p2 -> p1
            for (int i = 0; i < placements.Count; i++) {
                ProductPlacement placement1 = placements[i];
                if (Equals(placement1, skip)) continue;
                for (int j = i + 1; j < placements.Count; j++) {
                    ProductPlacement placement2 = placements[j];
                    if (Equals(placement2, skip)) continue;

                    (Position p1, Position p2) = PolyUtil.MinDistance(placement1.GetPoly(), placement2.GetPoly());

                    double distance = p1.Distance(p2);
                    if (distance >= 150) continue;
                    //Als het minder dan 150 cm is voegd die de lijn toe.
                    DistanceLine line = new DistanceLine(p1, p2);
                    line.Add(Editor);
                    _coronaLines.Add(line);
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

                // Zet de schaal van de ruimte op basis van de dimensies, dit moet na het zetten van het design
                SetRoomScale();
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void PlaceProduct(Product product, int x, int y) {
            // Checkt of het product niet null is en of de foto geplaatst mag worden
            if (product == null || !AllowDrop) return;
            ProductPlacements.Add(
                new ProductPlacement() {
                    Product = product,
                    X = x,
                    Y = y
                }
            );

            // Add product to product overview
            AddToOverview(product);
            CheckCorona();

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void TryToMoveProduct(ProductPlacement placement, int newX, int newY) {
            //Alleen als een object naar het nieuwe punt verplaatst mag worden, wordt het vervangen.
            if (!AllowDrop) return;

            //Verwijder de placement van de placement om te voorkomen dat het product verdubbeld wordt
            var index = ProductPlacements.FindIndex(element => Equals(element, placement));
            //Trek de helft van de hoogte en breedte van het product eraf
            //Zodat het product in het midden van de cursor staat
            placement.X = newX;
            placement.Y = newY;
            //Na het aanpassen wordt het weer toegevoegd om de illusie te geven dat het in de lijst wordt aangepast
            ProductPlacements[index] = placement;
            CheckCorona();
        }

        public void CanvasMouseDown(object sender, MouseButtonEventArgs e) {
            //Rechtermuisknop zorgt ervoor dat informatie over het product wordt getoond
            if (e.ChangedButton == MouseButton.Right) {
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
                var placement = (ProductPlacement) e.Data.GetData(typeof(ProductPlacement));
                Point position = e.GetPosition(Editor);
                var x = (int) position.X - (placement.Product.Width / 2);
                var y = (int) position.Y - (placement.Product.Length / 2);
                TryToMoveProduct(placement, x, y);
                _draggingPlacement = null;
                RenderRoom();
            }
        }

        public void CanvasDragOver(object sender, DragEventArgs e) {
            //Controleer of er een product is geselecteerd
            if (e.Data == null) return;
            Product selectedProduct = null;
            ProductPlacement skip = null;
            //Afhankelijk van het type data wordt de product op een andere manier opgehaald

            //Haal de positie van de cursor op
            Point position = e.GetPosition(Editor);

            if (e.Data.GetDataPresent(typeof(Product))) {
                selectedProduct = (Product) e.Data.GetData(typeof(Product));
            } else if (e.Data.GetDataPresent(typeof(ProductPlacement))) {
                ProductPlacement placement = (ProductPlacement) e.Data.GetData(typeof(ProductPlacement));
                skip = placement;
                selectedProduct = placement?.Product;
            }

            //Als de muis niet bewogen is hoeft het niet opnieuw getekend te worden
            if (position == _previousPosition) return;
            _previousPosition = position;

            CheckCorona(
                new ProductPlacement(
                    (int) position.X - selectedProduct.Width / 2, (int) position.Y - selectedProduct.Length / 2,
                    selectedProduct, null
                ), skip
            );

            // Check of het product in de ruimte wordt geplaatst
            AllowDrop = CheckRoomCollisions(RoomPoly.Points, position, selectedProduct);

            //Teken de ruimte en de al geplaatste producten
            RenderRoom();
            // Render het plaatje vna het product als de cursor binnen de polygon zit
            DrawProduct(
                selectedProduct,
                (int) position.X - (selectedProduct.Width / 2),
                (int) position.Y - (selectedProduct.Length / 2), transparent: !AllowDrop
            );
        }

        private void RenderRoom() {
            for (int i = Editor.Children.Count - 1; i >= 0; i += -1) {
                UIElement Child = Editor.Children[i];
                if (Child is Image) Editor.Children.Remove(Child);
            }

            for (int i = 0; i < ProductPlacements.Count; i++) {
                var placement = ProductPlacements[i];
                //Controleer of de placement op dat moment verplaatst wordt
                //Als dit het geval is moet de placement doorzichtig worden
                DrawProduct(placement.Product, placement.X, placement.Y, i, _draggingPlacement == placement);
            }

            if (_selectedPlacement != null) {
                DrawSelectionButtons(_selectedPlacement);
            }
        }

        private void DrawSelectionButtons(ProductPlacement placement) {
            PlacementSelectScreen selectScreen = new PlacementSelectScreen();
            selectScreen.DataContext = placement.Product;
            selectScreen.DeleteButton.Click += (o, e) => {
                ProductPlacements.Remove(placement);
                _selectedPlacement = null;
                RenderRoom();
            };
            Canvas.SetTop(selectScreen, placement.Y + placement.Product.Length);
            Canvas.SetLeft(selectScreen, placement.X);
            Editor.Children.Add(selectScreen);
        }

        public void DrawProduct(Product product, int x, int y, int? placementIndex = null, bool transparent = false) {
            //Haal de bestandsnaam van de foto op of gebruik de default
            var photo = product.Photo ?? "placeholder.png";
            var image = new Image() {
                Source = new BitmapImage(new Uri(Environment.CurrentDirectory + $"/Resources/Images/{photo}")),
                Height = product.Length,
                Width = product.Width
            };

            //Als transparent in als parameter naar true wordt gezet wordt de afbeelding doorzichtig
            if (transparent) image.Opacity = 0.5;


            Canvas.SetTop(image, y);
            Canvas.SetLeft(image, x);
            // Voeg product toe aan canvas
            Editor.Children.Add(image);
            // Voegt het id van het productplacement index in de productplacement list
            image.Uid ??= placementIndex.ToString();
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

        public bool CheckRoomCollisions(PointCollection vertices, Point point, Product product) {
            int j = vertices.Count() - 1;
            int yOffset = product.Length / 2;
            int xOffset = product.Width / 2;

            // Punten aanmaken waar om gecheckt moet worden
            PointCollection points = new PointCollection() {
                new Point(point.X - xOffset, point.Y - yOffset),
                new Point(point.X + xOffset, point.Y - yOffset),
                new Point(point.X - xOffset, point.Y + yOffset),
                new Point(point.X + xOffset, point.Y + yOffset),
            };

            foreach (Point p in points) {
                bool result = false;
                // Loopt door alle punten in de polygon
                for (int i = 0; i < vertices.Count(); i++) {
                    // Kijkt of de gegeven point in de polygon ligt qua coordinaten
                    if (vertices[i].Y < p.Y && vertices[j].Y >= p.Y || vertices[j].Y < p.Y && vertices[i].Y >= p.Y) {
                        if (vertices[i].X + (p.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) *
                            (vertices[j].X - vertices[i].X) < p.X) {
                            result = !result;
                        }
                    }

                    j = i;
                }

                if (!result) return false;
            }

            return true;
        }

        public void SetRoomScale() {
            double scale;

            // Zet de dimensies van de ruimte polygon
            RoomPoly.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            // Als de breedte hoger is dan de breedte wordt de breedte gebruikt voor de schaal en vice versa
            if (RoomPoly.DesiredSize.Width > RoomPoly.DesiredSize.Height) {
                scale = _canvasWidth / RoomPoly.DesiredSize.Width;
            } else {
                scale = _canvasHeight / RoomPoly.DesiredSize.Height;
            }

            ScaleCanvas(scale);
        }

        public void CanvasMouseScroll(object sender, MouseWheelEventArgs e) {
            ScaleCanvas(Scale + (e.Delta > 0 ? -0.02 : 0.02));
        }

        public void ResizePage(object sender, SizeChangedEventArgs e) {
            // Berekent voor de hoogte en breedte het canvas, de hoogte en breedte veranderd alleen als de room polygon kleiner wordt dan dat deze was 
            double width = _canvasWidth / RoomPoly.ActualWidth < Scale ? _canvasWidth / RoomPoly.ActualWidth : Scale;
            double height = _canvasHeight / RoomPoly.ActualHeight < Scale
                ? _canvasHeight / RoomPoly.ActualHeight
                : Scale;

            // De kleinste waarde wordt meegegeven aan de scale functie
            ScaleCanvas(width > height ? height : width);
        }

        private void ScaleCanvas(double scale) {
            // Kijkt of de gegeven schaal binnen de pagina past, zo niet veranderd de schaal niet
            //if (scale >= 0.01 && RoomPoly.ActualHeight * scale <= _canvasHeight && RoomPoly.ActualWidth * scale <= _canvasWidth)
            if (scale >= 0.01) {
                Scale = scale;
                Editor.RenderTransform = new ScaleTransform(scale, scale);
            }
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