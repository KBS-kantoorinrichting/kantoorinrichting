using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Designer.Model;
using Designer.Other;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Designer.ViewModel {
    public class ViewDesignViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<Product> Products { get; set; }
        public ArgumentCommand<DragEventArgs> DragDropCommand { get; set; }
        public ArgumentCommand<MouseButtonEventArgs> MouseDownCommand { get; set; }
        public Product SelectedProduct { get; set; }
        private Design Design { get; }
        public Canvas Editor { get; set; }


        //Is not used
        public ViewDesignViewModel(Design design) {
            Design = design;
            Products = LoadProducts();
            Editor = new Canvas();
            MouseDownCommand = new ArgumentCommand<MouseButtonEventArgs>(e => CatalogusMouseDown(this, e));
            DragDropCommand = new ArgumentCommand<DragEventArgs>(e => CanvasDragDrop(this, e));
        }

        //public List<ProductPlacement> ProductPlacements { get; set; } = new List<ProductPlacement>();

        //public List<Product> ProductList {
        //    get {
        //        List<Product> List = new List<Product>();

        //        for (int i = 0; i < 10; i++) {
        //            Product Product = new Product {
        //                ProductId = i,
        //                Name = $"Test {i}"
        //            };
        //            List.Add(Product);
        //        }

        //        return List;
        //    }
        //}

        public void SelectProduct(int id) {
            // Zet het geselecteerde product op basis van het gegeven ID
            Product Product = Products.Where(p => p.ProductId == id).FirstOrDefault();

            SelectedProduct = Product;
        }

        public void CatalogusMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Linker muisknop moet ingdrukt zijn
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // Cast datacontext naar int
                var obj = (int)((Border)sender).DataContext;

                // Init drag & drop voor geselecteerde product
                DragDrop.DoDragDrop(Editor, obj, DragDropEffects.Link);

                SelectProduct(obj);
            }
        }

        public void CanvasDragDrop(object sender, DragEventArgs e)
        {
            // Muispositie
            Point position = e.GetPosition((IInputElement)sender);

            // TODO: Add image of product
            var rect = new Rectangle() { Height = 10, Width = 10 };

            rect.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));

            // Voeg product toe aan canvas
            Editor.Children.Add(rect);

            // Zet posities van product
            Canvas.SetTop(rect, position.Y);
            Canvas.SetLeft(rect, position.X);
        }

        public static List<Product> LoadProducts() { 
            using var Context = new RoomDesignContext();
            return Context.Products.ToList();
        }
    }
}