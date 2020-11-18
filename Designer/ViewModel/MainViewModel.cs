﻿using System.ComponentModel;
using System.Windows;
using Designer.Other;
using Designer.View;

namespace Designer.ViewModel {
    public class MainViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        public BasicCommand GotoDesigns { get; set; }
        public BasicCommand GotoRooms { get; set; }
        public BasicCommand GotoExample { get; set; }
        public BasicCommand GotoProducts { get; set; }
        public BasicCommand Exit { get; set; }
        public Navigator Navigator { get; set; }

        public MainViewModel() {
            Navigator = Navigator.Instance;
            GotoDesigns = new PageCommand(() => {
                DesignCatalog DesignCatalog = new DesignCatalog();
                DesignCatalog.DesignSelected += (o, e) =>
                {
                    Navigator.Instance.Replace(new ViewDesignPage(e.Value));
                };
                return DesignCatalog;
            });
            GotoProducts = new PageCommand(() => new ViewProductsView());
            GotoDesigns = new PageCommand(() => new DesignCatalog());
            GotoRooms = new PageCommand(() => new RoomEditorView());
            GotoExample = new PageCommand(() => new ExamplePage());
            Exit = new BasicCommand(() => Application.Current.Shutdown());
        }

        private void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}