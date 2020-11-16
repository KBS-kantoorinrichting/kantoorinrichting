using System.Collections.Generic;

namespace Designer.Model
{
    public class Design
    {
        public int DesignId { get; set; }
        public string Name { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        public List<ProductPlacement> ProductPlacements { get; set; }

        public Design() { }

        public Design(string name, Room room, List<ProductPlacement> productPlacements) {
            Name = name;
            Room = room;
            ProductPlacements = productPlacements;
        }
    }
}
