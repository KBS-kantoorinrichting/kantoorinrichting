using System.Collections.Generic;

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
    }
}
