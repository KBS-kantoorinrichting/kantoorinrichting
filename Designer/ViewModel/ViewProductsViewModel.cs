using Designer.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Designer.Other;

namespace Designer.ViewModel {
    public class ViewProductsViewModel : INotifyPropertyChanged {
        public ArgumentCommand<MouseButtonEventArgs> MouseDownCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public int TestId { get; set; }
        public string TestName { get; set; }
        public double TestPrice { get; set; }
        public string TestDimensions { get; set; }
        public Product SelectedProduct { get; set; }

        public List<Model.Product> Products { get; set; }
        // Property van een lijst om de informatie vanuit de database op te slaan.

        public ViewProductsViewModel() {
            var context = RoomDesignContext.Instance;
            // Linq om te zorgen dat de lijst gevuld wordt met de database content.

            this.Products = context.Products.ToList();
            // this.Products is de lijst met producten
            // context.Products is de table Products van de database 
            int i = 0;
            foreach (var n in Products) {
                n.ProductId = i;
                i++;
            }

            TestPrice = GetPrice(GetId("Bureau"));
            TestName = GetName(GetId("Bureau"));
            TestDimensions = GetDimensions(GetId("Bureau"));
            MouseDownCommand =
                new ArgumentCommand<MouseButtonEventArgs>(e => CatalogusMouseDown(e.OriginalSource, e));
        }

        public void CatalogusMouseDown(object sender, MouseButtonEventArgs e) {
            // Linker muisknop moet ingdrukt zijn
            if (e.LeftButton == MouseButtonState.Pressed) {
/*                if (sender.GetType() != typeof(Image)) return;
                // Cast datacontext naar int
                var obj = (Product)((Image)sender).DataContext;
                //SelectedProduct = obj;
                SelectProduct(obj.ProductId);

                // Init drag & drop voor geselecteerde product
                DragDrop.DoDragDrop(Editor, obj, DragDropEffects.Link);*/
            }
        }

        #region Database_Fill_Code

        public void FillDataBase()
            // Functie om snel de database te vullen met test producten.
        {
            using (var context = RoomDesignContext.Instance) {
                context.Products.Add(new Model.Product("Bureaustoel", 51.30, ""));
                context.Products.Add(new Model.Product("Tuintafel", 200.00, ""));
                context.Products.Add(new Model.Product("Bureau", 140.40, ""));
                context.Products.Add(new Model.Product("Lamp", 30.60, ""));
                context.Products.Add(new Model.Product("Kleed", 80.00, ""));
                context.Products.Add(new Model.Product("Koffieapparaat", 600, ""));
                context.Products.Add(new Model.Product("Willem", 5.99, ""));

                context.SaveChanges();
            }
        }

        #endregion

        public string GetName(int id) {
            // Functie om naam op te halen met het ID
            var Name = from n in Products
                where n.ProductId == id
                select n.Name;

            return Name.First();
        }

        public int GetWidth(int id) {
            // Functie om naam op te halen met het ID
            var Width = from n in Products
                where n.ProductId == id
                select n.Width;

            return Width.First();
        }

        public int GetLength(int id) {
            // Functie om naam op te halen met het ID
            var Length = from n in Products
                where n.ProductId == id
                select n.Length;

            return Length.First();
        }

        public string GetDimensions(int id) { return ($"{GetLength(id)} x {GetWidth(id)}"); }

        public int GetId(string name) {
            // Functie om naam op te halen met het ID
            var Id = from n in Products
                where n.Name == name
                select n.ProductId;

            return Id.First();
        }

        public double GetPrice(int id)
            // Functie om de prijs op te halen van de producten
        {
            var Price = from p in Products
                where p.ProductId == id
                select p.Price;

            return Price.First().GetValueOrDefault(0);
        }

        public string GetPhoto(int id) {
            // Functie om de string van waar de foto van de producten staat op te halen
            var Photo = from p in Products
                where p.ProductId == id
                select p.Photo;

            return Photo.First();
        }

        public void EmptyDataBase()
            // Functie om de database te legen.    
        {
            using (var context = RoomDesignContext.Instance) {
                for (int i = 0; i < Products.Count; i++) {
                    context.Products.Remove(Products[i]);
                }

                context.SaveChanges();
            }
        }
    }
}