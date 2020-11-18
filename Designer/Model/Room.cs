namespace Designer.Model
{
    public class Room
    {
        public int RoomId { get; set; }
        public string Name { get; set; }

        public int Width { get; set; }
        public int Length { get; set; }

        public Room() { }

        public Room(int roomId, string name, int width, int length) {
            RoomId = roomId;
            Name = name;
            Width = width;
            Length = length;
        }
    }
}