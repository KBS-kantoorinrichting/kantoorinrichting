using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Designer.Model;

namespace Designer.ViewModel {
    
    //Command wat een actie uitvoert geen async
    public class DefaultCommand : ICommand {
        private readonly Action _action;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _action?.Invoke();

        public DefaultCommand(Action action) { _action = action; }

        public event EventHandler CanExecuteChanged;
    }
    
    public class AddDesignModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public List<Room> Rooms { get; set; }
        public Room Selected { get; set; }
        public string Name { get; set; }
        public ICommand Submit { get ; set; }

        //Wordt aangeroepen wanneer het design toegevoegd is
        public event EventHandler<DesignAddedArgs> DesignAdded;

        public AddDesignModel() {
            Rooms = LoadRooms();
            Submit = new DefaultCommand(AddDesign);
        }

        private void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddDesign() {
            if (Name == null || Selected == null) throw new ArgumentNullException();
            Design design = CreateDesign(Name, Selected);
            design = SaveDesign(design);
            DesignAdded?.Invoke(this, new DesignAddedArgs(design));
        }

        public static List<Room> LoadRooms() {
            return RoomDesignContext.Instance.Rooms.ToList();
        }
        
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