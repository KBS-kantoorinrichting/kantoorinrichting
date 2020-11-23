using Designer.Other;
using Designer.View;
using Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace Designer.ViewModel
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string TotalProducts => ProductService.Instance.Count().ToString();
        public string TotalDesigns => DesignService.Instance.Count().ToString();
        public string TotalRooms => RoomService.Instance.Count().ToString();
        public BasicCommand GotoDesigns { get; set; }
        public BasicCommand GotoProducts { get; set; }

        public HomeViewModel() {
            GotoDesigns = new PageCommand(() => new DesignCatalog());
            GotoProducts = new PageCommand(() => new ViewProductsView());
        }
    }
}
