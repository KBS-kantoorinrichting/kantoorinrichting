using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Models {
    public class RoomPlacement : Data, IEntity {
        private Polygon _polygon;

        public RoomPlacement() { }

        public RoomPlacement(string positions, int rotation, FrameTypes type) {
            Positions = positions;
            Rotation = rotation;
            Type = type;
        }

        public RoomPlacement(Room room, string positions, int rotation, FrameTypes type) : this(
            positions, rotation, type
        ) {
            Room = room;
        }

        public int RoomId { get; set; }
        public Room Room { get; set; }

        public string Positions {
            get => _polygon.Convert();
            set => _polygon = new Polygon(value);
        }

        public int Rotation { get; set; }

        [Column("Type")]
        public string TypeString {
            get => Type.ToString();
            private set => Type = (FrameTypes) Enum.Parse(typeof(FrameTypes), value, true);
        }

        [NotMapped] public FrameTypes Type { get; set; }

        protected override ITuple Variables => (Id, RoomId, Room, Positions, Rotation, Type);
        [Column("RoomPlacementId")] public int Id { get; set; }

        public Polygon GetPoly() { return _polygon; }

        public static string FromDimensions(int x, int y) {
            return Room.FromList(
                new[] {
                    new Position(x, y),
                    new Position(x + 25, y),
                    new Position(x + 25, y + 25),
                    new Position(x, y + 25)
                }
            );
        }

        public static Position ToPosition(string positions) {
            List<Position> list = positions.Split("|").Select(p => new Position(p)).ToList();

            return list[0];
        }

        //public Polygon FrameDimension()
        //{

        //}

        //public List<Position> GetFrameDimensions()
        //{
        //    List<Position> positions = new List<Position>();

        //    if (Type == FrameTypes.Door)
        //    {

        //    } else if(Type == FrameTypes.Window)
        //    {

        //    }

        //    return positions;
        //}
    }

    public enum FrameTypes {
        Door, Window
    }
}