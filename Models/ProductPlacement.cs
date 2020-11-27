using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Models {
    public class ProductPlacement : Data, IEntity {
        [Column("ProductPlacementId")] 
        public int Id { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int DesignId { get; set; }
        public Design Design { get; set; }

        protected override ITuple Variables => (Id, X, Y, ProductId, Product, DesignId, Design);

        public ProductPlacement() {
        }

        public ProductPlacement(int x, int y, Product product, Design design) {
            X = x;
            Y = y;
            Product = product;
            Design = design;
        }

        public List<Position> GetPoly() {
            List<Position> positions = Product.GetPoly();
            return positions.Select(p => new Position(p.X + X, p.Y + Y)).ToList();
        }
    }
}