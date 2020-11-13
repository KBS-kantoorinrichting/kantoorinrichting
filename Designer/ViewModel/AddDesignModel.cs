using System;
using System.Collections.Generic;
using System.ComponentModel;
using Designer.Model;

namespace Designer.ViewModel {
    public class AddDesignModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public List<Room> Rooms { get; set; } = new List<Room>();
        public Room Selected { get; set; }

        public string Name { get => "Hey"; set => Console.WriteLine(value); }

        public AddDesignModel() {
            Rooms.Add(new Room() {
                Name = "T1.07", Length = 10, Width = 10, RoomId = 1
            });
            Rooms.Add(new Room() {
                Name = "T1.08", Length = 10, Width = 10, RoomId = 5
            });
            Rooms.Add(new Room() {
                Name = "T1.09", Length = 10, Width = 10, RoomId = 2
            });
            Rooms.Add(new Room() {
                Name = "T1.10", Length = 10, Width = 10, RoomId = 9
            });
            Rooms.Add(new Room() {
                Name = "T1.11", Length = 10, Width = 10, RoomId = 3
            });
        }

        private void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}