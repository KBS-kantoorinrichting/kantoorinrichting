using System.Collections.Generic;
using System.Linq;

namespace Designer.Model {
    public class Room {
        public int RoomId { get; set; }
        public string Name { get; set; }

        public string Positions { get; set; }

        public Room() { }

        public Room(string name, string positions) {
            Name = name;
            Positions = positions;
        }

        public static List<Position> ToList(string positions) {
            return positions.Split("|")
                .Select(p => new Position(p))
                .ToList();
        }

        public static string FromList(IEnumerable<Position> positions) {
            return positions.Select(p => p.ToString())
                .Aggregate((s1, s2) => $"${s1}|${s2}");
        }

        public static Room FromDimensions(string name, int width, int height) {
            return new Room(name, FromList(new[] {
                new Position(0,0),
                new Position(width,0),
                new Position(width,height),
                new Position(0,height),
            }));
        }
    }
}