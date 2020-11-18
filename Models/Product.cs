using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Product : IEntity
    {
        [Column("ProductId")]
        public int Id { get; set; }
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
