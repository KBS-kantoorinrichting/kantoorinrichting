using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        #nullable enable
        public double? Price { get; set; }
        public string? Photo { get; set; }
        #nullable disable
    }
}
