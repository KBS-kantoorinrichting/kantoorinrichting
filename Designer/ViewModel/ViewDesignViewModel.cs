using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Designer.Model;
using Designer.Other;

namespace Designer.ViewModel {
    public class ViewDesignViewModel : INotifyPropertyChanged {
        public ViewDesignViewModel() {
            Test = new ArgumentCommand<DragEventArgs>(
                e => {
                    Console.WriteLine(e);
                }
            );
        }

        //Is not used
        public ViewDesignViewModel(Design design) {
            Design = design;
            ProductPlacements = design.ProductPlacements;
            Editor = new Canvas();
        }

        public List<ProductPlacement> ProductPlacements { get; set; } = new List<ProductPlacement>();

        public List<Product> ProductList {
            get {
                List<Product> List = new List<Product>();

                for (int i = 0; i < 10; i++) {
                    Product Product = new Product {
                        ProductId = i,
                        Name = $"Test {i}"
                    };
                    List.Add(Product);
                }

                return List;
            }
        }

        public Product SelectedProduct { get; set; }
        private Design Design { get; }
        public Canvas Editor { get; set; }
        public int Width { get; set; } = 200;
        public int Length { get; set; } = 500;
        public BasicCommand Test { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public void SelectProduct(int id) {
            // Zet het geselecteerde product op basis van het gegeven ID
            Product Product = ProductList.Where(p => p.ProductId == id).FirstOrDefault();

            SelectedProduct = Product;
        }

        public static List<Product> LoadProducts() { return RoomDesignContext.Instance.Products.ToList(); }
    }
}