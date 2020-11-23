using System.ComponentModel;
using System.Windows;
using Designer.Other;
using Designer.View;
using Services;

namespace Designer.ViewModel {
    public class MainViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public BasicCommand GotoDesigns { get; set; }
        public BasicCommand GotoRooms { get; set; }
        public BasicCommand GotoExample { get; set; }
        public BasicCommand Exit { get; set; }
        public Navigator Navigator { get; set; }

        public MainViewModel() {
            //Maak de db connectie aan
            RoomService.Instance.Get(0);
            
            Navigator = Navigator.Instance;
            GotoDesigns = new PageCommand(() => {
                DesignCatalog DesignCatalog = new DesignCatalog();
                DesignCatalog.DesignSelected += (o, e) =>
                {
                    Navigator.Instance.Replace(new ViewDesignPage(e.Value));
                };
                return DesignCatalog;
            });
            GotoRooms = new PageCommand(() => new RoomEditorView());
            GotoExample = new PageCommand(() => new ExamplePage());
            Exit = new BasicCommand(() => Application.Current.Shutdown());
        }

        private void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}