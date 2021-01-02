using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Designer.Utils;
using Designer.View;
using MaterialDesignThemes.Wpf;
using Models;
using Services;

namespace Designer.ViewModel {
    public class ViewDesigns : INotifyPropertyChanged {
        private Room _selected;

        public ViewDesigns() {
            Navigator = Navigator.Instance;
            Rooms = LoadRooms();
            EditCommand = new ArgumentCommand<Design>(
                design => {
                    Navigator.Push(new DesignEditorView(design));
                    ;
                }
            );
            DeleteCommand = new ArgumentCommand<Design>(DeleteDesign);
            CancelCommand = new BasicCommand(ClosePopup);
            AddDesignCommand = new BasicCommand(AddDesign);
            ReloadCommand = new BasicCommand(Reload);
            OpenPopup = new BasicCommand(
                () => {
                    IsAdding = true;
                    OnPropertyChanged();
                }
            );
            //Om te voorkomen dat unit tests falen
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA) {
                RenderDesigns();
                MessageQueue = new SnackbarMessageQueue();
            }
        }

        private Dictionary<Design, Canvas> _designs { get; set; }

        public Dictionary<Design, Canvas> Designs {
            get {
                if (SelectedFilter != null)
                    return _designs.Where(d => d.Key.Room.Id == SelectedFilter.Id)
                        .ToDictionary(d => d.Key, d => d.Value);
                return _designs;
            }
        }

        public List<Room> Rooms { get; set; }

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
        public bool IsAdding { get; set; }

        public Design Selected {
            get => null;
            set => GotoDesign(value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //Wordt aangeroepen wanneer er eem design geselecteerd is
        public event EventHandler<BasicEventArgs<Design>> DesignSelected;

        private void ClosePopup() {
            IsAdding = false;
            EnteredName = "";
            SelectedRoom = null;
            OnPropertyChanged();
        }

        private void DeleteDesign(Design design) {
            DesignService.Instance.Delete(design);
            MessageQueue.Enqueue("Het ontwerp is verwijderd");
            Reload();
            OnPropertyChanged();
        }

        private void AddDesign() {
            if (EnteredName == "" || SelectedRoom == null) return;
            Design design = new Design(EnteredName, SelectedRoom, new List<ProductPlacement>());
            DesignService.Instance.Add(design);
            DesignService.Instance.SaveChanges();
            IsAdding = false;
            EnteredName = "";
            SelectedRoom = null;
            Reload();
            MessageQueue.Enqueue("Het ontwerp is toegevoegd");
        }

        public void Reload() {
            RenderDesigns();
            Rooms = LoadRooms();
            OnPropertyChanged();
        }

        public void PageOnDesignAdded(object sender, BasicEventArgs<Design> e) {
            Navigator.Pop();
            Reload();
            GotoDesign(e.Value);
        }

        public void GotoDesign(Design design) { DesignSelected?.Invoke(this, new BasicEventArgs<Design>(design)); }

        public static List<Design> LoadDesigns() {
            //Doesn't load productplacements and products without this,
            //due to EF lazy loading
            ProductPlacementService.Instance.GetAll();
            ProductService.Instance.GetAll();
            return DesignService.Instance.GetAll();
        }

        public void RenderDesigns() {
            List<Design> designs = LoadDesigns();
            _designs = new Dictionary<Design, Canvas>();
            foreach (Design design in designs) {
                Canvas canvas = CanvasUtil.CreateRoomCanvas(design.Room);
                _designs.Add(design, CanvasUtil.FillCanvas(design, canvas));
            }
        }

        public static List<Room> LoadRooms() { return RoomService.Instance.GetAll(); }

        protected virtual void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}