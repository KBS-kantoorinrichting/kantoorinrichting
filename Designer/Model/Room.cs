using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designer.Model
{
    public class Room
    {
        public int RoomId { get; set; }
        public string Name { get; set; }

        public string Positions { get; set; }

        public Room() { }

        public Room(string name, string positions) {
            Name = name;
            Positions = positions;
        }

        public static Room FromDimensions(string name, int width, int height) {
            //TODO generate positionsd
            return new Room(name, "");
        }
    }
}
