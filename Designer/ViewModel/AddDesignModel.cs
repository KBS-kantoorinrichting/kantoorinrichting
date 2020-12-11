using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Designer.Other;
using Models;
using Services;

namespace Designer.ViewModel {
    public class AddDesignModel : INotifyPropertyChanged {
        //Wordt aangeroepen wanneer het design toegevoegd is
        public event EventHandler<BasicEventArgs<Design>> DesignAdded;
        public event PropertyChangedEventHandler PropertyChanged;

        public List<Room> Rooms { get; set; }
        private Room _selected;
        public Room Selected {
            get => _selected;
            set {
                _selected = value;
                Submit.Disabled = _selected == null || Name == null;
            }
        }

        private string _name;
        public string Name {
            get => _name;
            set {
                _name = value;
                Submit.Disabled = _selected == null || Name == null;
            }
        }
        private string _plexiglass;
        public string Plexiglass {
            get => _plexiglass;
            set {
                _plexiglass = value;
            }
        }
        
        public BasicCommand Submit { get; set; }
        public BasicCommand Cancel { get; set; }
        public string Error { get; set; }

        public AddDesignModel() {
            Rooms = LoadRooms();
            Submit = new BasicCommand(AddDesign, true);
            Cancel = new PageCommand(type: NavigationType.Pop);
        }

        private void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddDesign() {
            if (Name == null || Selected == null) {
                ShowError("Vul eerst alles in.");
                return;
            }

            Design design = CreateDesign(Name, Selected, Plexiglass);
            SaveDesign(design);
            DesignAdded?.Invoke(this, new BasicEventArgs<Design>(design));
        }

        public void ShowError(string error) {
            Error = error;
            OnPropertyChanged();
            Task.Delay(2000).ContinueWith(
                t => {
                    Error = "";
                    OnPropertyChanged();
                }
            );
        }

        public static List<Room> LoadRooms() {
            return RoomService.Instance.GetAll();
        }

        public static Design CreateDesign(string name, Room room, string plexiglass) {
            return new Design(name, room, new List<ProductPlacement>(), plexiglass);
        }

        public static Design SaveDesign(Design design) {
            design = DesignService.Instance.Save(design);
            return design;
        }
    }
}