using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Designer.Other;
using Designer.View;
using Models;
using Services;

namespace Designer.ViewModel {
    public class DesignCatalogModel : INotifyPropertyChanged {
        //Wordt aangeroepen wanneer er eem design geselecteerd is
        public event EventHandler<BasicEventArgs<Design>> DesignSelected;
        public event PropertyChangedEventHandler PropertyChanged;

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

        private AddDesign AddDesignPage() {
            //Maak de toevoegen design pagina aan en registeerd de event listeren
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
            Console.WriteLine(design);
            DesignSelected?.Invoke(this, new BasicEventArgs<Design>(design));
        }

        public static List<Design> LoadDesigns() {
            ProductPlacementService.Instance.GetAll();
            return DesignService.Instance.GetAll();
        }

        protected virtual void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}