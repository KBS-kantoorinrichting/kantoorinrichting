﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Designer.Other;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Designer.View;
using Models;
using Services;

namespace Designer.ViewModel
{
    public class ViewDesignViewModel : INotifyPropertyChanged
    {
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
        private ProductPlacement _draggingPlacement;

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
            CatalogusMouseDownCommand =
                new ArgumentCommand<MouseButtonEventArgs>(e => CatalogusMouseDown(e.OriginalSource, e));
            CanvasMouseDownCommand =
                new ArgumentCommand<MouseButtonEventArgs>(e => CanvasMouseDown(e.OriginalSource, e));
            DragDropCommand = new ArgumentCommand<DragEventArgs>(e => CanvasDragDrop(e.OriginalSource, e));
            DragOverCommand = new ArgumentCommand<DragEventArgs>(e => CanvasDragOver(e.OriginalSource, e));
            _productOverview = new Dictionary<Product, ProductData>();
        }

        public void SetDesign(Design design)
        {
            Design = design;
            ProductPlacements = design.ProductPlacements;
            ProductPlacements ??= new List<ProductPlacement>();
            _productOverview = new Dictionary<Product, ProductData>();
            //Wanneer niet in test env render die de ruimte
            if (Editor != null) RenderRoom();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void PlaceProduct(Product product, int x, int y)
        {
            if (product == null) return;
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
            //Rechtermuisknop zorgt ervoor dat informatie over het product wordt getoond
            if (e.ChangedButton == MouseButton.Right)
            {
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
            //Linkermuisknop betekent dat het product wordt verplaatst
            else
            {
                if (sender.GetType() != typeof(Image)) return;
                var image = sender as Image;
                var placement = ProductPlacements.Where(placement =>
                    placement.X == Canvas.GetLeft(image) && placement.Y == Canvas.GetTop(image));
                if (placement.Count() > 0)
                {
                    _draggingPlacement = placement.First();
                    DragDrop.DoDragDrop(Editor, _draggingPlacement, DragDropEffects.Move);
                }
            };
        }

        public void CatalogusMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Linker muisknop moet ingdrukt zijn
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender.GetType() != typeof(Image)) return;
                // Cast datacontext naar product
                var obj = (Product) ((Image) sender).DataContext;

                // Init drag & drop voor geselecteerde product
                DragDrop.DoDragDrop(Editor, obj, DragDropEffects.Link);
            }
        }

        public void CanvasDragDrop(object sender, DragEventArgs e)
        {
            //Als er geen product is geselecteerd, doe niks
            if (e.Data == null) return;
            //In dit geval wordt er een product toegevoegd
            if (e.Data.GetDataPresent(typeof(Product)))
            {
                var selectedProduct = (Product) e.Data.GetData(typeof(Product));
                Point position = e.GetPosition(Editor);
                //Trek de helft van de hoogte en breedte van het product eraf
                //Zodat het product in het midden van de cursor staat
                PlaceProduct(selectedProduct,
                    (int) (position.X - (selectedProduct.Width / 2)),
                    (int) (position.Y - (selectedProduct.Length / 2)));
                RenderRoom();
            }
            //Hier wordt een product dat al in het design zit verplaatst
            else if(e.Data.GetDataPresent(typeof(ProductPlacement)))
            {
                var placement = (ProductPlacement) e.Data.GetData(typeof(ProductPlacement));
                Point position = e.GetPosition(Editor);
                //Verwijder de placement van de placement om te voorkomen dat het product verdubbeld wordt
                ProductPlacements.Remove(placement);
                //Trek de helft van de hoogte en breedte van het product eraf
                //Zodat het product in het midden van de cursor staat
                placement.X = (int)position.X - (placement.Product.Width / 2);
                placement.Y = (int) position.Y - (placement.Product.Length / 2);
                //Na het aanpassen wordt het weer toegevoegd om de illusie te geven dat het in de lijst wordt aangepast
                ProductPlacements.Add(placement);
                _draggingPlacement = null;
                RenderRoom();
            }
        }

        public void CanvasDragOver(object sender, DragEventArgs e)
        {
            //Controleer of er een product is geselecteerd
            if (e.Data == null) return;
            Product selectedProduct = null;
            //Afhankelijk van het type data wordt de product op een andere manier opgehaald
            if (e.Data.GetDataPresent(typeof(Product)))
            {
                selectedProduct = (Product) e.Data.GetData(typeof(Product));
            }
            else if (e.Data.GetDataPresent(typeof(ProductPlacement)))
            {
                selectedProduct = (e.Data.GetData(typeof(ProductPlacement)) as ProductPlacement)?.Product;
            }
            //Haal de positie van de cursor op
            Point position = e.GetPosition(Editor);
            //Als de muis niet bewogen is hoeft het niet opnieuw getekend te worden
            if (position == _previousPosition) return;
            _previousPosition = position;
            //Teken de ruimte en de al geplaatste producten
            RenderRoom();
            DrawProduct(selectedProduct,
                (int) position.X - (selectedProduct.Width / 2),
                (int) position.Y - (selectedProduct.Length / 2)
            );
        }

        private void RenderRoom()
        {
            Editor.Children.Clear();
            for (int i = 0; i < ProductPlacements.Count; i++)
            {
                var placement = ProductPlacements[i];
                //Controleer of de placement op dat moment verplaatst wordt
                //Als dit het geval is moet de placement doorzichtig worden
                DrawProduct(placement.Product, placement.X, placement.Y, i, _draggingPlacement == placement);
            }

            if (_selectedPlacement != null)
            {
                DrawSelectionButtons(_selectedPlacement);
            }
        }

        private void DrawSelectionButtons(ProductPlacement placement)
        {
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

        public void DrawProduct(Product product, int x, int y, int? placementIndex = null, bool transparent = false)
        {
            //Haal de bestandsnaam van de foto op of gebruik de default
            var photo = product.Photo ?? "placeholder.png";
            var image = new Image()
            {
                Source = new BitmapImage(new Uri(@"pack://application:,,,/" + $"Resources/Images/{photo}")),
                Height = product.Length,
                Width = product.Width
            };
            
            //Als transparent in als parameter naar true wordt gezet wordt de afbeelding doorzichtig
            if (transparent)
                image.Opacity = 0.5;
            
            
            Canvas.SetTop(image, y);
            Canvas.SetLeft(image, x);
            // Voeg product toe aan canvas
            Editor.Children.Add(image);
            // Voegt het id van het productplacement index in de productplacement list
            image.Uid ??= placementIndex.ToString();
        }

        public static List<Product> LoadProducts() {
            return ProductService.Instance.GetAll();
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
                _productOverview.Add(product, new ProductData() {Total = 1, TotalPrice = price});
            }
        }
    }

    public class ProductData
    {
        public int Total { get; set; }
        public double TotalPrice { get; set; }
    }
}