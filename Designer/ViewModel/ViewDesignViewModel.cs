using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using Designer.Model;
using Designer.Other;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Designer.ViewModel {
    public class ViewDesignViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<Product> Products { get; set; }
        private Dictionary<Product, int> _productOverview { get; set; }
        public List<Product> ProductOverview
        {
            get => _productOverview.Keys.ToList();
        }
        public List<ProductPlacement> ProductPlacements { get; set; }
        public ArgumentCommand<DragEventArgs> DragDropCommand { get; set; }
        public ArgumentCommand<DragEventArgs> DragOverCommand { get; set; }
        public ArgumentCommand<MouseButtonEventArgs> MouseDownCommand { get; set; }
        public Product SelectedProduct { get; set; }
        private Design Design { get; set; }
        public Canvas Editor { get; set; }
        private Point _previousPosition;

        public ViewDesignViewModel()
        {
            Products = LoadProducts(); 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProductPlacements"));
            MouseDownCommand = new ArgumentCommand<MouseButtonEventArgs>(e => CatalogusMouseDown(e.OriginalSource, e));
            DragDropCommand = new ArgumentCommand<DragEventArgs>(e => CanvasDragDrop(e.OriginalSource, e));
            DragOverCommand = new ArgumentCommand<DragEventArgs>(e => CanvasDragOver(e.OriginalSource, e));
            _productOverview = new Dictionary<Product, int>();
        }

        public void SetDesign(Design design)
        {
            Design = design;
            ProductPlacements = design.ProductPlacements;
            Editor = new Canvas();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void PlaceProduct(int x, int y)
        {
            if (SelectedProduct == null) return;
            ProductPlacements.Add(new ProductPlacement()
            {
                Product = SelectedProduct,
                X = x,
                Y = y
            });

            // Add product to product overview
            AddToOverview(SelectedProduct);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProductPlacements"));
        }

        public void SelectProduct(int id) {
            // Zet het geselecteerde product op basis van het gegeven ID
            var list = Products.Where(p => p.ProductId == id).ToList();
            Product product = list.FirstOrDefault();
            SelectedProduct = product;
        }
        
        public void CatalogusMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Linker muisknop moet ingdrukt zijn
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if(sender.GetType() != typeof(Image)) return;
                // Cast datacontext naar int
                var obj = (Product)((Image)sender).DataContext;
                //SelectedProduct = obj;
                SelectProduct(obj.ProductId);
                
                // Init drag & drop voor geselecteerde product
                DragDrop.DoDragDrop(Editor, obj, DragDropEffects.Link);
            }
        }

        public void CanvasDragDrop(object sender, DragEventArgs e)
        {
            if (SelectedProduct == null) return;
            Point position = e.GetPosition((IInputElement)Editor);
            PlaceProduct(
                (int)(position.X - (SelectedProduct.Width / 2)), 
                (int)(position.Y - (SelectedProduct.Length / 2)));
        }
        
        public void CanvasDragOver(object sender, DragEventArgs e)
        {
            //Controleer of er een product is geselecteerd
            if (SelectedProduct == null) return;
            //Haal de positie van de cursor op
            Point position = e.GetPosition((IInputElement)Editor);
            //Als de muis niet bewogen is hoeft het niet opnieuw getekend te worden
            if (position == _previousPosition) return;
            _previousPosition = position;
            //Teken de ruimte en de al geplaatste producten
            RenderRoom();
            DrawProduct(SelectedProduct, 
                (int)position.X - (SelectedProduct.Width / 2),
                (int)position.Y - (SelectedProduct.Length / 2)
                );
        }
        
        private void RenderRoom()
        {
            Editor.Children.Clear();
            foreach (var placement in ProductPlacements)
            {
                DrawProduct(placement.Product, placement.X, placement.Y);
            }
        }

        private void DrawProduct(Product product, int X, int Y)
        {
            var photo = product.Photo ?? "test.jpg";
            // TODO: Add image of product
            var image = new Image()
            {
                Source = new BitmapImage(new Uri(@"pack://application:,,,/" + $"Resources/Images/{photo}")),
                Height = product.Length,
                Width = product.Width
            };
                
            Canvas.SetTop(image, Y);
            Canvas.SetLeft(image, X);
                
            // Voeg product toe aan canvas
            Editor.Children.Add(image); 
        }

        public static List<Product> LoadProducts() { 
            using var Context = new RoomDesignContext();
            return Context.Products.ToList();
        }

        private void AddToOverview(Product product)
        {
            if (_productOverview.ContainsKey(product))
            {
                _productOverview[product] = _productOverview[product] + 1;
            } else
            {
                _productOverview.Add(product, 1);
            }
        }
    }
}