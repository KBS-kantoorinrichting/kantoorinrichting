using System;
using System.Collections.Generic;
using System.Text;

namespace Designer.Model
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        #nullable enable
        public double? Price { get; set; }
        public string? Photo { get; set; }
        #nullable disable
        public int Width { get; set; }
        public int Length { get; set; }
        public List<ProductPlacement> ProductPlacements { get; set; }

        public Product()
        // Lege constructor voor product
        {

        }

        public Product(int id, string name, double price, string photo)
        // Alternatieve constructor voor Product
        {
            ProductId = id;
            Name = name;
            Price = price;
            Photo = photo;
        }
    }
}
