using System;
using System.Windows.Controls;
using Designer.ViewModel;

namespace Designer.View {
    public partial class AddDesign : Page {
        //Wordt aangeroepen wanneer het design toegevoegd is
        public event EventHandler<DesignAddedArgs> DesignAdded;
        
        public AddDesign() {
            InitializeComponent();
            if (!(DataContext is AddDesignModel model)) return;
            model.DesignAdded += (sender, args) => DesignAdded?.Invoke(sender, args);
        }
    }
}