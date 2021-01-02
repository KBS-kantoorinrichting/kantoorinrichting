using System.ComponentModel;
using Designer.Utils;
using Designer.View;
using Services;

namespace Designer.ViewModel {
    public class Home : INotifyPropertyChanged {
        public Home() {
            GotoDesigns = new PageCommand(
                () => {
                    ViewDesignsView DesignCatalog = new ViewDesignsView();
                    DesignCatalog.DesignSelected += (o, e) => {
                        Navigator.Instance.Replace(new DesignEditorView(e.Value));
                    };
                    return DesignCatalog;
                }
            );
            GotoProducts = new PageCommand(() => new ViewProductsView());
            GotoRooms = new PageCommand(() => new ViewRoomsView());
        }

        public string TotalProducts => ProductService.Instance.Count().ToString();
        public string TotalDesigns => DesignService.Instance.Count().ToString();
        public string TotalRooms => RoomService.Instance.Count().ToString();
        public BasicCommand GotoDesigns { get; set; }
        public BasicCommand GotoProducts { get; set; }
        public BasicCommand GotoRooms { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}