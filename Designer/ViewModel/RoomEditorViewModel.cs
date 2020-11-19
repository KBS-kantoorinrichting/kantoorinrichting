using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Data;
using Designer.Model;
using Designer.Other;
using Designer.View;

namespace Designer.ViewModel {
    public class RoomEditorViewModel : INotifyPropertyChanged  {
        public string Name { get; set; }
        public string Width { get; set; }
        public string Length { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public static int Template { get; set; }
        public static int x;
        public static int y;
        public static string Position;

        public string Image0
        {
            get; set;
        } = "../Assets/Vierhoek_Clicked.jpg";
        public string Image1
        {
            get; set;
        } = "../Assets/Hoekvormig.jpg";

        public BasicCommand Submit { get; set; }
        public BasicCommand TemplateButton { get; set; }
       

        public RoomEditorViewModel() {
            Submit = new BasicCommand(SubmitRoom);
            TemplateButton = new ArgumentCommand<int>(SetTemplate);
            // bind het command
        }

        private void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetTemplate(int parameter)
        {
           
            Template = parameter;
            if (parameter == 0)
            {
                Image0 = "../Assets/Vierhoek_Clicked.jpg";
                Image1 = "../Assets/Hoekvormig.jpg";
               
            }
            else
            {
                Image0 = "../Assets/Vierhoek.jpg";
                Image1 = "../Assets/Hoekvormig_Clicked.jpg";
                
            }
            OnPropertyChanged();

           
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
            Room room = Room.FromDimensions(name, width, length);
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
                Position += x + "," + y;

                // from deminetions variatie maken en de list ipv string

                room.Positions = Position;

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