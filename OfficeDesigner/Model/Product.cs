using System;
using System.Collections.Generic;
using System.Text;

namespace OfficeDesigner.Model
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public double? Price { get; set; }
        public string Photo { get; set; }

        public List<ProductPlacement> ProductPlacements { get; set; }
    }
}
