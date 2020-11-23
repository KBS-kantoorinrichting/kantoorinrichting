﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Designer.Model;
using Designer.Other;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Designer.View;
using System.Windows.Shapes;
using System.Windows.Media;
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
        public Product SelectedProduct => _selectedPlacement.Product;
        public Design Design { get; set; }
        public Canvas Editor { get; set; }
        private Point _previousPosition;
        private ProductPlacement _selectedPlacement;
        public Polygon RoomPoly { get; set; }
        public bool AllowDrop = false;

        //Special constructor for unit tests
        public ViewDesignViewModel(Design design)
        {
            SetDesign(design);
            Products = LoadProducts();
        }
        
        public ViewDesignViewModel()
        {
            Products = LoadProducts(); 
            Editor = new Canvas();
            RoomPoly = new Polygon();
            CatalogusMouseDownCommand = new ArgumentCommand<MouseButtonEventArgs>(e => CatalogusMouseDown(e.OriginalSource, e));
            CanvasMouseDownCommand = new ArgumentCommand<MouseButtonEventArgs>(e => CanvasMouseDown(e.OriginalSource, e));
            DragDropCommand = new ArgumentCommand<DragEventArgs>(e => CanvasDragDrop(e.OriginalSource, e));
            DragOverCommand = new ArgumentCommand<DragEventArgs>(e => CanvasDragOver(e.OriginalSource, e));
            _productOverview = new Dictionary<Product, ProductData>();

            // Sets the dimensions of the current room
            SetRoomDimensions();
        }

        public void SetDesign(Design design)
        {
            Design = design;
            ProductPlacements = new List<ProductPlacement>();
            ProductPlacements = design.ProductPlacements;
            _productOverview = new Dictionary<Product, ProductData>();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void PlaceProduct(Product product, int x, int y)
        {
            // Checkt of het product niet null is en of de foto geplaatst mag worden
            if (product == null || !AllowDrop) return;
            ProductPlacements.Add(new ProductPlacement()
            {
                Product = product,
                X = x,
                Y = y
            });
            
            // Add product to product overview
            AddToOverview(product);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void CanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            if (sender.GetType() == typeof(Canvas))
            {
                _selectedPlacement = null;
                RenderRoom();
            }
            if (sender.GetType() != typeof(Image)) return;
            var image = sender as Image;
            var placement = ProductPlacements.Where(placement =>
                placement.X == Canvas.GetLeft(image) && placement.Y == Canvas.GetTop(image));
            if (placement.Count() > 0)
            {
                _selectedPlacement = placement.First();    
            }
            
            RenderRoom();
        }
        
        public void CatalogusMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Linker muisknop moet ingdrukt zijn
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if(sender.GetType() != typeof(Image)) return;
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
            var selectedProduct = (Product)e.Data.GetData(typeof(Product));
            Point position = e.GetPosition(Editor);
            //Trek de helft van de hoogte en breedte van het product eraf
            //Zodat het product in het midden van de cursor staat
            PlaceProduct(selectedProduct,
                (int)(position.X - (selectedProduct.Width / 2)), 
                (int)(position.Y - (selectedProduct.Length / 2)));
        }
        
        public void CanvasDragOver(object sender, DragEventArgs e)
        {
            //Controleer of er een product is geselecteerd
            if (e.Data == null) return;
            var selectedProduct = (Product)e.Data.GetData(typeof(Product)); 
            //Haal de positie van de cursor op
            Point position = e.GetPosition(Editor);
            //Als de muis niet bewogen is hoeft het niet opnieuw getekend te worden
            if (position == _previousPosition) return;
            _previousPosition = position;

            // Check of het product in de ruimte wordt geplaatst
            AllowDrop = CheckRoomCollisions(RoomPoly.Points, position);

            //Teken de ruimte en de al geplaatste producten
            RenderRoom();
            // Render het plaatje vna het product als de cursor binnen de polygon zit
            if(AllowDrop)
                DrawProduct(selectedProduct, 
                    (int)position.X - (selectedProduct.Width / 2),
                    (int)position.Y - (selectedProduct.Length / 2)
                    );
        }
        
        private void RenderRoom()
        {
            for (int i = Editor.Children.Count - 1; i >= 0; i += -1)
            {
                UIElement Child = Editor.Children[i];
                if (!(Child is Polygon))
                    Editor.Children.Remove(Child);
            }

            for(int i = 0; i < ProductPlacements.Count; i++)
            {
                var placement = ProductPlacements[i];
                DrawProduct(placement.Product, placement.X, placement.Y, i);
            }

            if (_selectedPlacement != null)
            {
                DrawSelectionButtons(_selectedPlacement);
            }
        }

        private void DrawSelectionButtons(ProductPlacement placement)
        {
            /*
            Button deleteButton = new Button();
            deleteButton.Content = "X";
            Canvas.SetTop(deleteButton, placement.Y);
            Canvas.SetLeft(deleteButton, placement.X);
            deleteButton.Click += (o, e) =>
            {
                ProductPlacements.Remove(placement);
                _selectedPlacement = null;
                RenderRoom();
            };
            Editor.Children.Add(deleteButton);
            */
            PlacementSelectScreen selectScreen = new PlacementSelectScreen();
            selectScreen.DataContext = placement.Product;
            selectScreen.DeleteButton.Click += (o, e) =>
            {
                ProductPlacements.Remove(placement);
                _selectedPlacement = null;
                RenderRoom();
            };
            Canvas.SetTop(selectScreen, placement.Y + placement.Product.Length);
            Canvas.SetLeft(selectScreen, placement.X);
            Editor.Children.Add(selectScreen);
        }

        public void DrawProduct(Product product, int x, int y, int? placementIndex = null)
        {
            //Haal de bestandsnaam van de foto op of gebruik de default
            var photo = product.Photo ?? "placeholder.png";
            var image = new Image()
            {
                Source = new BitmapImage(new Uri(@"pack://application:,,,/" + $"Resources/Images/{photo}")),
                Height = product.Length,
                Width = product.Width
            };
           
            Canvas.SetTop(image, y);
            Canvas.SetLeft(image, x);
            // Voeg product toe aan canvas
            Editor.Children.Add(image);
            // Voegt het id van het productplacement index in de productplacement list
            image.Uid ??= placementIndex.ToString();
        }

        public static List<Product> LoadProducts() { 
            var context = RoomDesignContext.Instance;
            return context.Products.ToList();
        }

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
            // TODO: Replace with room positions
            var coordinates = ConvertPosititionsToCoordinates("0,0|250,0|250,250|500,250|500,500|0,500");


            PointCollection points = new PointCollection();
            // Voeg de punten toe aan een punten collectie
            for (int i = 0; i < coordinates.Count; i++) {
                points.Add(new Point(coordinates[i].X, coordinates[i].Y));
            }

            RoomPoly.Stroke = Brushes.Black;
            RoomPoly.Fill = Brushes.LightGray;
            RoomPoly.StrokeThickness = 1;
            RoomPoly.HorizontalAlignment = HorizontalAlignment.Left;
            RoomPoly.VerticalAlignment = VerticalAlignment.Center;
            RoomPoly.Points = points;
            Editor.Children.Add(RoomPoly);
        }

        public List<Coordinate> ConvertPosititionsToCoordinates(string positions)
        {
            List<Coordinate> values = new List<Coordinate>();

            // Splits de posities per pipe
            string[] coordinates = positions.Split("|");

            // Loop door alle coordinaten en voeg deze vervolgens toe aan values variable
            foreach(string coordinate in coordinates)
            {
                string[] val = coordinate.Split(",");
                values.Add(new Coordinate(Convert.ToInt32(val[0]), Convert.ToInt32(val[1])));
            }

            return values;
        }

        public bool CheckRoomCollisions(PointCollection vertices, Point point)
        {
            int j = vertices.Count() - 1;
            int yOffset = 60;
            int xOffset = 60;
            Debug.WriteLine(point.X);
            Debug.WriteLine(point.Y);

            // Punten aanmaken waar om gecheckt moet worden
            PointCollection points = new PointCollection()
            {
                new Point(point.X - xOffset, point.Y - yOffset),
                new Point(point.X + yOffset, point.Y - xOffset),
                new Point(point.X - yOffset, point.Y + yOffset),
                new Point(point.X + xOffset, point.Y + xOffset),
            };

            foreach (Point p in points)
            {
                bool result = false;
                // Loopt door alle punten in de polygon
                for (int i = 0; i < vertices.Count(); i++)
                {
                    // Kijkt of de gegeven point in de polygon ligt qua coordinaten
                    if (vertices[i].Y < p.Y && vertices[j].Y >= p.Y || vertices[j].Y < p.Y && vertices[i].Y >= p.Y)
                    {
                        if (vertices[i].X + (p.Y - vertices[i].Y) / (vertices[j].Y - vertices[i].Y) * (vertices[j].X - vertices[i].X) < p.X)
                        {
                            result = !result;
                        }
                    }
                    j = i;
                }
                if (!result) return false;
            }
            return true;
        }
    }

    public class ProductData
    {
        public int Total { get; set; }
        public double TotalPrice { get; set; }
    }

    public class Coordinate
    {
        public int X;
        public int Y;

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Coordinate(Point point)
        {
            X = Convert.ToInt32(point.X);
            Y = Convert.ToInt32(point.Y);
        }
    }
}