using System;
using System.Windows.Controls;
using Designer.Model;
using Designer.Other;
using Designer.View;

namespace Designer.ViewModel {
    public class RoomEditorViewModel {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
        public BasicCommand Submit { get; set; }

        public RoomEditorViewModel() { Submit = new BasicCommand(SubmitRoom); }

        public void SubmitRoom() {
            // opslaan van de ruimte als het aan de condities voldoet
            if (SaveRoom(Name, Width, Length) != null) {
                //opent successvol dialoog
                RoomEditorPopupView popup = new RoomEditorPopupView("De kamer is opgeslagen!");
                popup.ShowDialog();
            } else {
                //opent onsuccesvol dialoog
                RoomEditorPopupView popup = new RoomEditorPopupView("Er is iets misgegaan! probeer opnieuw.");
                popup.ShowDialog();
            }
        }

        // methode om de kamer op te slaan
        public static Room SaveRoom(string name, int width, int length) {
            // voegt de specificaties van de kamer aan het object room toe
            Room room = new Room();
            room.Name = name;
            room.Width = width;
            room.Length = length;

            // kamer opslaan
            var context = RoomDesignContext.Instance;
            room = context.Rooms.Add(room).Entity;

            try {
                context.SaveChanges();
                return room;
            } catch (Exception e) {
                Console.WriteLine(e);
                return null;
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