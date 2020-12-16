using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Models {
    public class Design : Data, IEntity {
        [Column("DesignId")] public int Id { get; set; }

        public string Name { get; set; }
        public string Plexiglass { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        public List<ProductPlacement> ProductPlacements { get; set; } = new List<ProductPlacement>();

        public Design() { }

        public Design(string name, Room room, List<ProductPlacement> productPlacements, string plexiglass = "") {
            Name = name;
            Plexiglass = plexiglass;
            Room = room;
            ProductPlacements = productPlacements;
        }

        public string Route {
            get => _route?.Convert();
            set => _route = new Polygon(value);
        }
        
        private Polygon _route;

        public Polygon GetRoutePoly() {
            return _route;
        }
        
        protected override ITuple Variables => (Id, Name, RoomId, Room, ProductPlacements, Plexiglass, Route);
    }
}