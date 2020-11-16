using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Designer.Model;

namespace Designer.ViewModel {
    public class RoomEditorViewModel {
        public Room room = new Room();

        public int Width { get; set; }

        public int Length { get; set; }

        public RoomEditorViewModel() { }

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

    public class StringToIntValidationRule : ValidationRule {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo) {
            if (int.TryParse(value?.ToString(), out int _)) return new ValidationResult(true, null);
            return new ValidationResult(false, "Please enter a valid integer value.");
        }
    }
}