using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Models {
    public class Room : Data, IEntity {
        [Column("RoomId")] 
        public int Id { get; set; }

        public string Name { get; set; }

        public int Width { get; set; }

        public int Length { get; set; }

        public Room(string name = default, int width = default, int length = default, int id = default) {
            Id = id;
            Name = name;
            Width = width;
            Length = length;
        }

        protected override ITuple Variables => (Id, Name, Width, Length);
    }
}