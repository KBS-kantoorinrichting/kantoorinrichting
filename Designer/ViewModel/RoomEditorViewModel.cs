using System;
using System.Text.RegularExpressions;
using Designer.Model;

namespace Designer.ViewModel {
    public class RoomEditorViewModel {
        public Room room = new Room();

        public long Width { get; set; }
        public long Length { get; set; }

        // gebruik regex om te kijken of je text letters bevat (voor lengte en breedte)
        private static readonly Regex Regex = new Regex("[^0-9.-]+");
        public static bool IsNumber(string text) { return !Regex.IsMatch(text); }

        // methode om de kamer op te slaan
        public bool SaveRoom(string name, int width, int length) {
            // voegt de specificaties van de kamer aan het object room toe
            room.Name = name;
            room.Width = width;
            room.Length = length;
            
            // kamer opslaan
            var context = RoomDesignContext.Instance;
            var post = context.Rooms.Add(room);

            try {
                context.SaveChanges();
                return true;
            } catch (Exception e) {
                Console.WriteLine(e);
                return false;
                throw;
            }
        }

    }
}
