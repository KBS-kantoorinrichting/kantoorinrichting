﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
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

        [Column("Type")]
        public string TypeString
        {
            get { return Type.ToString(); }
            private set { Type = (FrameTypes)Enum.Parse(typeof(FrameTypes), value, true); }
        }

        [NotMapped]
        public FrameTypes Type { get; set; }

        protected override ITuple Variables => (Id, RoomId, Room, Positions, Rotation, Type);

        private Polygon _polygon;

        public RoomPlacement() { }

        public RoomPlacement(string positions, int rotation, FrameTypes type)
        {
            Positions = positions;
            Rotation = rotation;
            Type = type;
        }

        public RoomPlacement(Room room, string positions, int rotation, FrameTypes type) : this(positions, rotation, type)
        {
            Room = room;
        }

        public Polygon GetPoly() { return _polygon; }
    }

    public enum FrameTypes
    {
        Door,
        Window
    }
}