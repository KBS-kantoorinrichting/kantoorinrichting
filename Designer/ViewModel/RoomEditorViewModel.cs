using System;
using System.ComponentModel;
using System.Windows.Controls;
using Designer.Model;
using Designer.Other;
using Designer.View;

namespace Designer.ViewModel  {
    public class RoomEditorViewModel{
        public string Name { get; set; }
        public string Width { get; set; }
        public string Length { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public BasicCommand Submit { get; set; }

        public RoomEditorViewModel() { Submit = new BasicCommand(SubmitRoom); }

            public void SubmitRoom() {

            if (int.TryParse(Width, out int width) && int.TryParse(Length, out int length))
            {
                // opslaan van de ruimte als het aan de condities voldoet
                if (SaveRoom(Name, width, length) != null)
                {
                    //opent successvol dialoog
                    RoomEditorPopupView popup = new RoomEditorPopupView("De kamer is opgeslagen!");
                    popup.ShowDialog();
                    return;
                }
                
            } 
                //opent onsuccesvol dialoog
                RoomEditorPopupView popupfalse = new RoomEditorPopupView("Er is iets misgegaan! probeer opnieuw.");
                popupfalse.ShowDialog();
            

            
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

    // data type validatie voor de lengte
    public class StringToIntValidationRule : ValidationRule {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo) {
            if (value.ToString().Equals("") || value.ToString().Equals(null))
            {
                if (int.TryParse(value?.ToString(), out int _)) return new ValidationResult(true, null);
                return new ValidationResult(false, "");
                
            }
            else
            {
                if (int.TryParse(value?.ToString(), out int _)) return new ValidationResult(true, null);
                return new ValidationResult(false, "Dit veld mag alleen cijfers bevatten.");
            }
            
        }
    }




}