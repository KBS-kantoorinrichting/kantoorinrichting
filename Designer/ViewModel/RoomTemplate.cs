﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using Designer.Utils;
using MaterialDesignThemes.Wpf;
using Models;
using Services;

namespace Designer.ViewModel {
    public class RoomTemplate : INotifyPropertyChanged {
        public static int x;
        public static int y;
        public static string Position;

        public RoomTemplate() {
            // submit command van submitknop
            Submit = new BasicCommand(SubmitRoom);
            // bind het templatecommand van de templateknoppen
            TemplateButton = new ArgumentCommand<int>(SetTemplate);
            MessageQueue = new SnackbarMessageQueue();
        }

        public string Name { get; set; }
        public string Width { get; set; }
        public string Length { get; set; }

        public static int Template { get; set; }

        // image toevoegen aan knop
        public string Image0 { get; set; } = "Assets/Vierhoek_Clicked.jpg";
        public string Image1 { get; set; } = "Assets/Hoekvormig.jpg";

        public BasicCommand Submit { get; set; }
        public BasicCommand TemplateButton { get; set; }
        public SnackbarMessageQueue MessageQueue { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = "") {
            // herlaad de hele pagina
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetTemplate(int parameter) {
            // geeft de parameter waarde door aan template
            Template = parameter;
            if (parameter == 0) {
                // image styling (welke ingedrukt is)
                Image0 = "Assets/Vierhoek_Clicked.jpg";
                Image1 = "Assets/Hoekvormig.jpg";
            } else {
                // image styling (welke ingedrukt is)
                Image0 = "Assets/Vierhoek.jpg";
                Image1 = "Assets/Hoekvormig_Clicked.jpg";
            }

            // herlaad pagina
            OnPropertyChanged();
        }

        public void SubmitRoom() {
            if (int.TryParse(Width, out int width) && int.TryParse(Length, out int length)) {
                if (Template == 1) {
                    if (SaveRoom(Name, width, length, Template) != null) {
                        //opent successvol dialoog
                        MessageQueue.Enqueue("De kamer is opgeslagen.");
                        return;
                    }

                    return;
                }

                if (SaveRoom(Name, width, length) != null) {
                    //opent successvol dialoog
                    MessageQueue.Enqueue("De kamer is opgeslagen.");
                    return;
                }

                // opslaan van de ruimte als het aan de condities voldoet
            }

            MessageQueue.Enqueue("Er is iets misgegaan! probeer opnieuw.");
        }

        // methode om de kamer op te slaan
        public static Room SaveRoom(string name, int width, int length) {
            // voegt de specificaties van de kamer aan het object room toe
            Room room = Room.FromDimensions(name, width, length);
            // kamer opslaan

            try {
                room = RoomService.Instance.Save(room);
                return room;
            } catch (Exception e) {
                Console.WriteLine(e);
                return null;
            }
        }

        public static Room SaveRoom(string name, int width, int length, int template) {
            // voegt de specificaties van de kamer aan het object room toe
            Room room = Room.FromTemplate(name, width, length, template);

            try {
                // kamer opslaan
                RoomService.Instance.Save(room);
                return room;
            } catch (Exception e) {
                Console.WriteLine(e);
                return null;
            }
        }
    }

    // data type validatie voor de lengte
    public class StringToIntValidationRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (value?.ToString() == null || value.ToString().Equals(""))
                return new ValidationResult(false, "Dit veld is verplicht");

            bool isInt = int.TryParse(value?.ToString(), out int _);
            return new ValidationResult(isInt, isInt ? null : "Dit veld mag alleen cijfers bevatten.");
        }
    }
}