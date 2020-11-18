﻿namespace Models
{
    public class ProductPlacement : IEntity
    {
        public int ProductPlacementId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int DesignId { get; set; }
        public Design Design { get; set; }

        public int Id => ProductPlacementId;
    }
}
