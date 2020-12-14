using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Models {
    public class Design : Data, IEntity {
        [Column("DesignId")] public int Id { get; set; }

        public string Name { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        public List<ProductPlacement> ProductPlacements { get; set; } = new List<ProductPlacement>();

        public Design() { }

        public Design(string name, Room room, List<ProductPlacement> productPlacements) {
            Name = name;
            Room = room;
            ProductPlacements = productPlacements;
        }

        public string Route {
            get => _route?.Convert();
            set => _route = new Polygon(value);
        }
        
        private Polygon _route;

        public Polygon GetRoutePoly() {
            List<Position> positions = new List<Position>();
            Position offset = Room.GetPoly().Max().CopyMultiple(0.05, 0.05);
            foreach (Position position in Room.GetPoly()) {
                positions.Add(new Position((int) (position.X * 0.9), (int) (position.Y * 0.9)));
            }
            return new Polygon(positions).Offset(offset);
            
            
            return _route;
        }
        
        protected override ITuple Variables => (Id, Name, RoomId, Room, ProductPlacements);
    }
}