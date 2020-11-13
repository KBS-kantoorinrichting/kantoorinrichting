using Designer.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

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

        public ViewDesignViewModel()
        {
        }

    }
}
