using Designer.Model;
using Designer.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Designer.View
{
    /// <summary>
    /// Interaction logic for ViewDesignPage.xaml
    /// </summary>
    public partial class ViewDesignPage : Page
    {
        public Window ParentWindow { get; set; }
        private ViewDesignViewModel ViewModel { get; set; }
        public ViewDesignPage(Design Design)
        {
            this.ViewModel = new ViewDesignViewModel(Design);
            ViewModel.PropertyChanged += (o,e) => RenderRoom();
            InitializeComponent();
        }

        private void Page_Initialized(object sender, EventArgs e)
        {
            RenderRoom();
        }

        private void RenderRoom()
        {
            Editor.Children.Clear();
            var room = new Rectangle() {Height = ViewModel.Length, Width = ViewModel.Width};
            room.Fill = new SolidColorBrush(Color.FromRgb(0, 255,0));
            Canvas.SetTop(room, 0);
            Canvas.SetLeft(room, 0);
            Editor.Children.Add(room);
            foreach (var placement in ViewModel.ProductPlacements)
            {
                var rect = new Rectangle() {Height = 10, Width = 10};
                rect.Fill = new SolidColorBrush(Color.FromRgb(255,0,0));
                Editor.Children.Add(rect);
                Canvas.SetTop(rect, placement.X);
                Canvas.SetLeft(rect, placement.Y); 
            }
        }

    }
}
