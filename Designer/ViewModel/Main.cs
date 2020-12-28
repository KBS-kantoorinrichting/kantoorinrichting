using System.ComponentModel;
using System.Windows;
using Designer.Other;
using Designer.View;
using Services;

namespace Designer.ViewModel {
    public class Main : INotifyPropertyChanged {
        public Main() {
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            //Maak de db connectie aan
            RoomService.Instance.Get(0);

            Navigator = Navigator.Instance;

            //Listen naar propertychanged events van de navigator om de navigation bar te updaten
            Navigator.PropertyChanged += OnNavigatorChange;

            Navigator.Push(new HomeView());
            GotoHome = new PageCommand(() => new HomeView());
            GotoProducts = new PageCommand(() => new ViewProductsView());
            GotoDesigns = new PageCommand(
                () => {
                    ViewDesignsView DesignCatalog = new ViewDesignsView();
                    DesignCatalog.DesignSelected += (o, e) => {
                        Navigator.Instance.Replace(new DesignEditorView(e.Value));
                    };
                    return DesignCatalog;
                }
            );
            GotoRooms = new PageCommand(() => new ViewRoomsView());
        }

        public BasicCommand GotoHome { get; set; }
        public BasicCommand GotoDesigns { get; set; }
        public BasicCommand GotoRooms { get; set; }
        public BasicCommand GotoExample { get; set; }
        public BasicCommand Save { get; set; }
        public BasicCommand GotoProducts { get; set; }
        public BasicCommand Exit { get; set; }
        public Navigator Navigator { get; set; }

        public bool OnHome => Navigator.CurrentPage.GetType() == typeof(HomeView);
        public bool OnProducts => Navigator.CurrentPage.GetType() == typeof(ViewProductsView);
        public bool OnDesigns => Navigator.CurrentPage.GetType() == typeof(ViewDesignsView);
        public bool OnRooms => Navigator.CurrentPage.GetType() == typeof(ViewRoomsView);
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnNavigatorChange(object o, PropertyChangedEventArgs e) { OnPropertyChanged(); }

        private void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}