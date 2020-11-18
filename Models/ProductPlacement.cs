using System.ComponentModel.DataAnnotations.Schema;

namespace Models {
    public class ProductPlacement : IEntity {
        [Column("ProductPlacementId")] 
        public int Id { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int DesignId { get; set; }
        public Design Design { get; set; }
    }
}