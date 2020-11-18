using System;
using System.Windows.Controls;
using Designer.Model;
using Designer.Other;
using Designer.View;

namespace Designer.ViewModel {
    public class RoomEditorViewModel {
        public string Name { get; set; }
        public string Width { get; set; }
        public string Length { get; set; }
        public int Template { get; set; }
        public static int x;
        public static int y;
        public static string Position;

        public BasicCommand Submit { get; set; }

        public RoomEditorViewModel() {
            Submit = new BasicCommand(SubmitRoom);
        }
        public void Btn1_Checked()
        {
            Template = 0;
        }
         public void Btn2_Checked()
        {
            Template = 1;
        }


        public void SubmitRoom() {

                if (int.TryParse(Width, out int width) && int.TryParse(Length, out int length))
                {
                    // opslaan van de ruimte als het aan de condities voldoet
                    if (SaveRoom(Name, width, length, Template) != null)
                    {
                        //opent successvol dialoog
                        RoomEditorPopupView popup = new RoomEditorPopupView("De kamer is opgeslagen!");
                        popup.ShowDialog();
                        return;
                    }
                }


            //opent onsuccesvol dialoog
            RoomEditorPopupView popupError = new RoomEditorPopupView("Er is iets misgegaan! probeer opnieuw.");
            popupError.ShowDialog();
        }

        // methode om de kamer op te slaan
        public static Room SaveRoom(string name, int width, int length, int template) {
            // voegt de specificaties van de kamer aan het object room toe
            if (template == 1)
            {
                x = 0;
                y = 0;
                Position = x + "," + y + "|";
                x = (width / 2);
                Position += x + "," + y + "|";
                y = (length / 2);
                Position += x + "," + y + "|";
                x = (width);
                Position += x + "," + y + "|";
                y = (length);
                Position += x + "," + y + "|";
                x = x - width;
                Position += x + "," + y + "|";

                Room room = Room(name, Position);
            }
            else
            {
                Room room = Room.FromDimensions(name, width, length);
            }
            

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
            if (value?.ToString() == null || value.ToString().Equals("") ) {
                return new ValidationResult(false, "Dit veld is verplicht");
            }

            bool isInt = int.TryParse(value?.ToString(), out int _);
            return new ValidationResult(isInt, isInt ? null : "Dit veld mag alleen cijfers bevatten.");
        }
    }
}