using System.ComponentModel;
using System.Windows;
using Designer.Other;
using Designer.View;
using Services;

namespace Designer.ViewModel {
    public class MainViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public BasicCommand GotoDesigns { get; set; }
        public BasicCommand GotoHome { get; set; }
        public BasicCommand GotoRooms { get; set; }
        public BasicCommand GotoExample { get; set; }
        public BasicCommand Save { get; set; }
        public BasicCommand GotoProducts { get; set; }
        public BasicCommand Exit { get; set; }
        public Navigator Navigator { get; set; }

        public MainViewModel() {
            //Maak de db connectie aan
            RoomService.Instance.Get(0);
            
            Navigator = Navigator.Instance;
            Navigator.Push(new HomeView());
            GotoDesigns = new PageCommand(() => {
                DesignCatalog DesignCatalog = new DesignCatalog();
                DesignCatalog.DesignSelected += (o, e) =>
                {
                    Navigator.Instance.Replace(new ViewDesignPage(e.Value));
                };
                return DesignCatalog;
            });
            GotoHome = new PageCommand(() => new HomeView());
            GotoRooms = new PageCommand(() => new RoomOverview());
            GotoProducts = new PageCommand(() => new ViewProductsView());
            GotoExample = new PageCommand(() => new ExamplePage());
            //Slaat alle aanpassing op
            Save = new BasicCommand(() => DesignService.Instance.SaveChanges());
            Exit = new BasicCommand(() => Application.Current.Shutdown());
        }
       
        private void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}