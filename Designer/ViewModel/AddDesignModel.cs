using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Designer.Model;

namespace Designer.ViewModel {
    //Command wat een actie uitvoert geen async
    public class DefaultCommand : ICommand {
        private readonly Action _action;
        private bool _disabled;
        public bool Disabled {
            get => _disabled;
            set {
                _disabled = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter) => !Disabled;
        public void Execute(object parameter) => _action?.Invoke();

        public DefaultCommand(Action action, bool disabled = false) {
            _action = action;
            Disabled = disabled;
        }

        public event EventHandler CanExecuteChanged;
    }

    public class AddDesignModel : INotifyPropertyChanged {
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
        public DefaultCommand Submit { get; set; }
        public string Error { get; set; }

        //Wordt aangeroepen wanneer het design toegevoegd is
        public event EventHandler<DesignAddedArgs> DesignAdded;

        public AddDesignModel() {
            Rooms = LoadRooms();
            Submit = new DefaultCommand(AddDesign, true);
        }

        private void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddDesign() {
            if (Name == null || Selected == null) {
                ShowError("Vul eerst alles in.");
                return;
            }

            Design design = CreateDesign(Name, Selected);
            design = SaveDesign(design);
            DesignAdded?.Invoke(this, new DesignAddedArgs(design));
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

        public static List<Room> LoadRooms() { return RoomDesignContext.Instance.Rooms.ToList(); }

        public static Design CreateDesign(string name, Room room) {
            return new Design(name, room, new List<ProductPlacement>());
        }

        public static Design SaveDesign(Design design) {
            RoomDesignContext context = RoomDesignContext.Instance;
            design = context.Add(design).Entity;
            context.SaveChanges();
            return design;
        }
    }

    public class DesignAddedArgs {
        public Design Design { get; }

        public DesignAddedArgs(Design design) { Design = design; }
    }
}