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
        public BasicCommand GotoRooms { get; set; }

        public HomeViewModel() {
            GotoRooms = new BasicCommand(NavigateToRooms);
        }

        public void NavigateToRooms() {
            Debug.WriteLine("test");
            Navigator.Instance.Push(new DesignCatalog());
        }
    }
}
