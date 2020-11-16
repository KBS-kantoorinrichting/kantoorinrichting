using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Designer.Model;
using Designer.Other;
using Designer.View;

namespace Designer.ViewModel {
    public class DesignCatalogModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<BasicEventArgs<Design>> DesignSelected;

        public List<Design> Designs { get; set; }
        public BasicCommand AddDesign { get; set; }
        public BasicCommand ReloadCommand { get; set; }
        public Navigator Navigator { get; set; }

        public Design Selected {
            get => null;
            set => GotoDesign(value);
        }

        public DesignCatalogModel() {
            Navigator = Navigator.Instance;
            Designs = LoadDesigns();
            AddDesign = new PageCommand(AddDesignPage, NavigationType.Push);
            ReloadCommand = new BasicCommand(Reload);
        }

        public AddDesign AddDesignPage() {
            AddDesign page = new AddDesign();
            page.DesignAdded += PageOnDesignAdded;
            return page;
        }

        public void Reload() {
            Designs = LoadDesigns();
            OnPropertyChanged();
        }

        public void PageOnDesignAdded(object sender, BasicEventArgs<Design> e) {
            Navigator.Pop();
            Reload();
            GotoDesign(e.Value);
        }

        public void GotoDesign(Design design) {
            DesignSelected?.Invoke(this, new BasicEventArgs<Design>(design));
        }

        public static List<Design> LoadDesigns() { return RoomDesignContext.Instance.Designs.ToList(); }

        protected virtual void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}