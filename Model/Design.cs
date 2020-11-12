using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class Design
    {
        public int DesignId { get; set; }
        public string Name { get; set; }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        public List<ProductPlacement> ProductPlacements { get; set; }
    }
}
