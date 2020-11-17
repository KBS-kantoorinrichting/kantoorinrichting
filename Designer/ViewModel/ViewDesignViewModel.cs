using Designer.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Designer.ViewModel
{
    public class ViewDesignViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public List<ProductPlacement> ProductPlacements { get; set; } = new List<ProductPlacement>();
        public List<Product> ProductList
        {
            get
            {
                List<Product> List = new List<Product>();

                for (int i = 0; i < 10; i++)
                {
                    Product Product = new Product
                    {
                        ProductId = i,
                        Name = $"Test {i}"
                    };
                    List.Add(Product);
                }
                
                return List;
            }
        }
        public Product SelectedProduct { get; set; }
        private Design Design { get; set; }
        public Canvas Editor { get; set; }
        public int Width { get; set; } = 200;
        public int Length { get; set; } = 500;

        public ViewDesignViewModel() { }
        public ViewDesignViewModel(Design design)
        {
            Design = design;
            ProductPlacements = design.ProductPlacements;
            Editor = new Canvas();
        }

        public void SelectProduct(int id)
        {
            // Zet het geselecteerde product op basis van het gegeven ID
            Product Product = ProductList.Where((p) => p.ProductId == id).FirstOrDefault();

            SelectedProduct = Product;
        }

        public static List<Product> LoadProducts() { return RoomDesignContext.Instance.Products.ToList(); }
    }
}
