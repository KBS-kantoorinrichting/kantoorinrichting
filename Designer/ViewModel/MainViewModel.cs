using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Designer.Model;
using Designer.Other;
using Designer.View;

namespace Designer.ViewModel {
    public class MainViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public List<Room> Rooms { get; set; }
        public List<Design> Designs { get; set; }

        public BasicCommand GotoDesigns { get; set; }
        public BasicCommand GotoExample { get; set; }
        public BasicCommand Exit { get; set; }

        public Navigator Navigator { get; set; }

        public MainViewModel() {
            RoomDesignContext context = RoomDesignContext.Instance;
            Rooms = context.Rooms.ToList();
            Designs = context.Designs.ToList();

            Navigator = Navigator.Instance;
            GotoDesigns = new PageCommand(() => new DesignCatalog());
            GotoExample = new PageCommand(() => new ExamplePage());
            Exit = new BasicCommand(() => Application.Current.Shutdown());
        }

        private void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}