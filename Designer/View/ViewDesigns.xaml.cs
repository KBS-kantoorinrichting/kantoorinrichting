using System;
using System.Windows.Controls;
using Designer.Other;
using Designer.ViewModel;
using Models;

namespace Designer.View {
    public partial class ViewDesignsView : Page {
        //Wordt aangeroepen wanneer er eem design geselecteerd is
        public event EventHandler<BasicEventArgs<Design>> DesignSelected;

        public ViewDesignsView() {
            InitializeComponent();
            if (!(DataContext is ViewDesigns model)) throw new NotSupportedException();
            model.DesignSelected += (sender, args) => DesignSelected?.Invoke(sender, args);
        }
    }
}