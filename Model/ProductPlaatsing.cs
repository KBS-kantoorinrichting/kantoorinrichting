using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class ProductPlaatsing
    {
        public int ProductPlaatsingId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int InrichtingId { get; set; }
        public Inrichting Inrichting { get; set; }
    }
}
