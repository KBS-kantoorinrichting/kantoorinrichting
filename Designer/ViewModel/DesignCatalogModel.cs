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

        private List<Design> _designs { get; set; }

        public List<Design> Designs
        {
            get
            {
                if (SelectedFilter != null)
                    return _designs.Where(d => d.Room.Id == SelectedFilter.Id).ToList();
                else
                    return _designs;
            }
        }

        public List<Room> Rooms { get; set; }
        private Room _selected = null;
        public Room SelectedFilter {
            get => _selected;
            set {
                _selected = value;
                OnPropertyChanged();
            }
        }
        
        public BasicCommand AddDesign { get; set; }
        public BasicCommand ReloadCommand { get; set; }
        public BasicCommand ClearFilterCommand { get; set; }
        public Navigator Navigator { get; set; }

        public Design Selected {
            get => null;
            set => GotoDesign(value);
        }

        public DesignCatalogModel() {
            Navigator = Navigator.Instance;
            _designs = LoadDesigns();
            Rooms = LoadRooms();
            AddDesign = new PageCommand(AddDesignPage, NavigationType.Push);
            ReloadCommand = new BasicCommand(Reload);
            ClearFilterCommand = new BasicCommand(ClearFilter);
        }

        private AddDesign AddDesignPage() {
            //Maak de toevoegen design pagina aan en registeerd de event listeren
            AddDesign page = new AddDesign();
            page.DesignAdded += PageOnDesignAdded;
            return page;
        }

        public void Reload() {
            _designs = LoadDesigns();
            Rooms = LoadRooms();
            OnPropertyChanged();
        }

        public void ClearFilter()
        {
            _selected = null;
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

        public static List<Design> LoadDesigns() {
            //Doesn't load productplacements without this, it's weird
            ProductPlacementService.Instance.GetAll();
            return DesignService.Instance.GetAll();
        }

        public static List<Room> LoadRooms()
        {
            return RoomService.Instance.GetAll();
        }

        protected virtual void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}