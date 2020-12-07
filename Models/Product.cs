using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Models {
    public class Product : Data, IEntity {
        [Column("ProductId")] public int Id { get; set; }
        public string Name { get; set; }

#nullable enable
        public double? Price { get; set; }
        public string? Photo { get; set; }
#nullable disable

        public int Width { get; set; }
        public int Length { get; set; }
        public bool HasPerson { get; set; }

        protected override ITuple Variables => (Id, Name, Price, Photo, Width, Length, HasPerson);

        public Product(
            string name = default,
            int id = default,
            int width = default,
            int length = default,
            double? price = default,
            string? photo = default,
            bool hasPerson = default

        ) {
            Id = id;
            Name = name;
            Price = price;
            Photo = photo;
            Width = width;
            Length = length;
            HasPerson = hasPerson;
            
        }
    }
}