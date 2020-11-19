using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Models
{
    public class Product : Data, IEntity
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
        
        protected override ITuple Variables => (Id, Name, Price, Photo, Width, Length);
    }
}
