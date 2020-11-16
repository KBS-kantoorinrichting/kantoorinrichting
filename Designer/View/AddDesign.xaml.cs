using System;
using System.Windows.Controls;
using Designer.Model;
using Designer.Other;
using Designer.ViewModel;

namespace Designer.View {
    public partial class AddDesign : Page {
        //Wordt aangeroepen wanneer het design toegevoegd is
        public event EventHandler<BasicEventArgs<Design>> DesignAdded;
        
        public AddDesign() {
            InitializeComponent();
            if (!(DataContext is AddDesignModel model)) throw new NotSupportedException();
            model.DesignAdded += (sender, args) => DesignAdded?.Invoke(sender, args);
        }
    }
}