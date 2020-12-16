using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using Designer.Other;
using Designer.Utils;
using Designer.View;
using MaterialDesignThemes.Wpf;
using Models;
using Services;

namespace Designer.ViewModel {
    public class DesignCatalogModel : INotifyPropertyChanged {
        //Wordt aangeroepen wanneer er eem design geselecteerd is
        public event EventHandler<BasicEventArgs<Design>> DesignSelected;
        public event PropertyChangedEventHandler PropertyChanged;

        private Dictionary<Design, Canvas> _designs { get; set; }

        public Dictionary<Design, Canvas> Designs
        {
            get
            {
                if (SelectedFilter != null)
                    return _designs.Where(d => d.Key.Room.Id == SelectedFilter.Id)
                        .ToDictionary(d => d.Key, d => d.Value);
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

        public ArgumentCommand<Design> EditCommand { get; set; }
        public ArgumentCommand<Design> DeleteCommand { get; set; }
        public BasicCommand CancelCommand { get; set; } 
        public BasicCommand AddDesignCommand { get; set; }
        public BasicCommand ReloadCommand { get; set; }
        public SnackbarMessageQueue MessageQueue { get; set; }
        public Navigator Navigator { get; set; }
        
        public string EnteredName { get; set; }
        public Room SelectedRoom { get; set; }
        public BasicCommand OpenPopup { get; set; }
        public bool IsAdding { get; set; } = false;

        public Design Selected {
            get => null;
            set => GotoDesign(value);
        }

        public DesignCatalogModel() {
            Navigator = Navigator.Instance;
            LoadDesigns();
            Rooms = LoadRooms();
            EditCommand = new ArgumentCommand<Design>(design =>
            {
                Navigator.Push(new ViewDesignPage((Design)design.Clone()));;
            });
            DeleteCommand = new ArgumentCommand<Design>(DeleteDesign);
            CancelCommand = new BasicCommand(ClosePopup);
            AddDesignCommand = new BasicCommand(AddDesign);
            ReloadCommand = new BasicCommand(Reload);
            OpenPopup = new BasicCommand(() =>
            {
                IsAdding = true;
                OnPropertyChanged();
            });
            MessageQueue = new SnackbarMessageQueue();
        }

        private void ClosePopup()
        {
            IsAdding = false;
            EnteredName = "";
            SelectedRoom = null;
            OnPropertyChanged();
        }

        private void DeleteDesign(Design design)
        {
            DesignService.Instance.Delete(design);
            MessageQueue.Enqueue("Het ontwerp is verwijderd");
            Reload();
            OnPropertyChanged();
        }

        private void AddDesign()
        {
            if (EnteredName == "" || SelectedRoom == null)
                return;
            var design = new Design(EnteredName, SelectedRoom, new List<ProductPlacement>());
            DesignService.Instance.Add(design);
            DesignService.Instance.SaveChanges();
            IsAdding = false;
            EnteredName = "";
            SelectedRoom = null;
            Reload();
            MessageQueue.Enqueue("Het ontwerp is toegevoegd");
        }

        private AddDesign AddDesignPage() {
            //Maak de toevoegen design pagina aan en registeerd de event listeren
            AddDesign page = new AddDesign();
            page.DesignAdded += PageOnDesignAdded;
            return page;
        }

        public void Reload() {
            LoadDesigns();
            Rooms = LoadRooms();
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

        public void LoadDesigns() {
            //Doesn't load productplacements without this, it's weird
            ProductPlacementService.Instance.GetAll();
            ProductService.Instance.GetAll();
            List<Design> designs = DesignService.Instance.GetAll();
            _designs = new Dictionary<Design, Canvas>();
            foreach (var design in designs)
            {
                var canvas = CanvasUtil.CreateRoomCanvas(design.Room);
                _designs.Add(design, CanvasUtil.FillCanvas(design, canvas));
            }
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