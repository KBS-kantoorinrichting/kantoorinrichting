using System.Windows;
using Designer.Other;

namespace Designer.View
{
    /// <summary>
    /// Interaction logic for GeneralPopup.xaml
    /// </summary>
    public partial class GeneralPopup : Window
    {
        public string Message { get; set; }
        public string WindowTitle { get; set; }
        public BasicCommand Continue { get; set; }

        public GeneralPopup(string message) : this(message, "Popup") { }

        public GeneralPopup(string message, string title)
        {
            InitializeComponent();
            DataContext = this;
            Message = message;
            WindowTitle = title;
            Title = title;
            Continue = new BasicCommand(Continue_Button_Click);
        }

        private void Continue_Button_Click()
        {
            Close();
        }
    }
}