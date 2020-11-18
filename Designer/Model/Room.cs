using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designer.Model
{
    public class Room : IEntity
    {
        public int RoomId { get; set; }
        public string Name { get; set; }

        public int Width { get; set; }

        public int Length { get; set; }

        public Room() { }

        public Room(string name, int width, int length) {
            Name = name;
            Width = width;
            Length = length;
        }

        public int Id => RoomId;
    }
}
