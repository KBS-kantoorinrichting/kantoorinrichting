using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Models
{
    public class RoomPlacement : Data, IEntity
    {
        [Column("RoomPlacementId")] public int Id { get; set; }

        private Polygon _poly;
        private int _rotation;

        public int RoomId { get; set; }
        public Room Room { get; set; }

        public string Positions
        {
            get => _polygon.Convert();
            set => _polygon = new Polygon(value);
        }

        public int Rotation
        {
            get => _rotation;
            set
            {
                _rotation = value;
                _poly = null;
            }
        }

        protected override ITuple Variables => (Id, RoomId, Room, Positions, Rotation);

        private Polygon _polygon;

        public RoomPlacement(string positions, int rotation)
        {
            Positions = positions;
            Rotation = rotation;
        }

        public RoomPlacement(Room room, string positions, int rotation)
        {
            Room = room;
            Positions = positions;
            Rotation = rotation;
        }

        public Polygon GetPoly() { return _polygon; }

        public static List<Position> ToList(string positions)
        {
            return positions switch
            {
                null => null,
                "" => new List<Position>(),
                _ => positions.Split("|").Select(p => new Position(p)).ToList()
            };
        }

        public static string FromList(IEnumerable<Position> positions)
        {
            if (positions == null) return null;
            IEnumerable<Position> enumerable = positions.ToList();

            if (!enumerable.Any()) return "";
            return enumerable.Select(p => p.ToString())
                .Aggregate((s1, s2) => $"{s1}|{s2}");
        }
    }
}