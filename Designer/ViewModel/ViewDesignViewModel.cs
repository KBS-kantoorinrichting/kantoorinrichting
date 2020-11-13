using Designer.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace Designer.ViewModel
{
    public class ViewDesignViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public List<Product> ProductList
        {
            get
            {
                List<Product> List = new List<Product>();

                for (int i = 0; i < 10; i++)
                {
                    Product Product = new Product
                    {
                        Name = $"Test {i}"
                    };
                    List.Add(Product);
                }
                
                return List;
            }
        }
        private Design Design { get; set; }
        public List<ProductPlacement> ProductPlacements { get; set; } = new List<ProductPlacement>();

        public int Width { get; set; } = 200;
        public int Length { get; set; } = 500;

        public ViewDesignViewModel() { }
        public ViewDesignViewModel(Design design)
        {
            Design = design;
            ProductPlacements = design.ProductPlacements;
        }

    }
}
