﻿using System.Collections.Generic;
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

        public int Rotation { get; set; }

        protected override ITuple Variables => (Id, X, Y, ProductId, Product, DesignId, Design, Rotation);

        public ProductPlacement() {
        }

        public ProductPlacement(int x, int y, Product product, Design design) {
            X = x;
            Y = y;
            Product = product;
            Design = design;
            Rotation = 0;
        }

        public Polygon GetPoly() => 
            Rotation % 180 == 0 ?
                Product.GetPoly().Offset(X, Y) :
                //Hoogte en breedte omgedraaid
                new Polygon(Product.Length, Product.Width).Offset(X,Y);
    }
}