using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Designer.Model;

namespace Designer.ViewModel {
    public class DefaultCommand : ICommand {
        private readonly Action _action;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _action?.Invoke();

        public DefaultCommand(Action action) { _action = action; }

        public event EventHandler CanExecuteChanged;
    }
    
    public class AddDesignModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public List<Room> Rooms { get; set; } = new List<Room>();
        public Room Selected { get; set; }
        public string Name { get; set; }
        public ICommand Submit { get ; set; }

        private RoomDesignContext _context;

        public AddDesignModel() {
            _context = new RoomDesignContext();
            Rooms = _context.Rooms.ToList();
            Submit = new DefaultCommand(() => _submit());
        }

        private void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task _submit() {
            Design design = new Design(Name, Selected, new List<ProductPlacement>());
            await _context.Designs.AddAsync(design);
            await _context.SaveChangesAsync();
            Console.WriteLine("Added");
        }
    }
}