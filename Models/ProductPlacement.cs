using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Models {
    public class ProductPlacement : Data, IEntity {
        [Column("ProductPlacementId")] 
        public int Id { get; set; }

        public int X {
            get => _x;
            set {
                _x = value;
                _poly = null;
            }
        }

        public int Y {
            get => _y;
            set {
                _y = value;
                _poly = null;
            }
        }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int DesignId { get; set; }
        public Design Design { get; set; }

        public int Rotation { get; set; }

        protected override ITuple Variables => (Id, X, Y, ProductId, Product, DesignId, Design, Rotation);

        public ProductPlacement() {
        }

        public ProductPlacement(Position position, Product product, Design design) : this(position.X, position.Y, product, design) {
        }

        public ProductPlacement(int x, int y, Product product, Design design = null) {
            X = x;
            Y = y;
            Product = product;
            Design = design;
            Rotation = 0;
        }

        private Polygon _poly;
        private int _x;
        private int _y;
        public Polygon GetPoly() => _poly ??= Product.GetPoly().Offset(X, Y);
    }
}