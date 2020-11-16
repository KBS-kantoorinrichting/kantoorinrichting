using System.ComponentModel;
using System.Windows;
using Designer.Other;
using Designer.View;

namespace Designer.ViewModel {
    public class MainViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public BasicCommand GotoDesigns { get; set; }
        public BasicCommand GotoExample { get; set; }
        public BasicCommand Exit { get; set; }

        public Navigator Navigator { get; set; }

        public MainViewModel() {
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