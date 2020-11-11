using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class ProductPlacement
    {
        public int ProductPlacementId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int DesignId { get; set; }
        public Design Design { get; set; }
    }
}
