using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using Designer.Model;

namespace Designer.ViewModel
{
    public class ViewDesignViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private Design Design { get; set; }
        public List<ProductPlacement> ProductPlacements { get; set; } = new List<ProductPlacement>();

        public int Width { get; set; } = 200;
        public int Length { get; set; } = 500;

        public ViewDesignViewModel(Design design)
        {
            Design = design;
            ProductPlacements = design.ProductPlacements;
        }

    }
}
