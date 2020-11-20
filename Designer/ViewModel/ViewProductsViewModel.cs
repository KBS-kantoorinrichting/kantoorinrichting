using System;
using Designer.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Designer.Other;
using System.Windows.Controls;
using Designer.View;
using Microsoft.Win32;
using Designer;
using System.Diagnostics;

namespace Designer.ViewModel
{
    public class ViewProductsViewModel : INotifyPropertyChanged {
        public string Name { get; set; }
        public double Price { get; set; }
        public string? Photo { get; set; }
        public int Width { get; set; }
        public int Length { get; set; } 
        public BasicCommand Submit { get; set; }
        public BasicCommand DeleteCommand { get; set; }
        public BasicCommand AddPhoto { get; set; }

        public BasicCommand ReloadCommand { get; set; }
        public ArgumentCommand<MouseButtonEventArgs> MouseDownCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public Product SelectedProduct { get; set; }


        public List<Model.Product> Products { get; set; }
        // Property van een lijst om de informatie vanuit de database op te slaan.

        public ViewProductsViewModel() {
            Reload();
            MouseDownCommand = new ArgumentCommand<MouseButtonEventArgs>(e => CatalogusMouseDown(e.OriginalSource, e));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedProduct"));
            AddPhoto = new BasicCommand(SelectPhoto);
            Submit = new BasicCommand(SubmitItem);
            ReloadCommand = new BasicCommand(Reload);
            DeleteCommand = new BasicCommand(Delete);
        }

        public void Reload()
        { // Reload de items zodat de juiste te zien zijn
            Products = LoadItems(Products);
            OnPropertyChanged();
        }

        public void SelectPhoto()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"{Environment.CurrentDirectory}\Resources\Images\";
            Debug.WriteLine(openFileDialog.InitialDirectory);
            if (openFileDialog.ShowDialog() == true)
                // Als deze open is dan:
            {
                openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*";
                Photo = openFileDialog.FileName.Split(@"\").Last();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Photo"));
                // De foto wordt veranderd in de applicatie
                
            }
        }
        public void Delete()
        {
            var context = RoomDesignContext.Instance;
            
                try
                {
                    context.Products.Remove(SelectedProduct);
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e);
                }

                context.SaveChanges();
                
                Reload();
            


           
        }


        public void CatalogusMouseDown(object sender, MouseButtonEventArgs e) {
            // Linker muisknop moet ingdrukt zijn
            if (e.LeftButton == MouseButtonState.Pressed) {
                if (sender.GetType() != typeof(Image)) return;
                var obj = (Product)((Image)sender).DataContext;
                //SelectedProduct = obj;
                SelectProduct(obj.ProductId);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedProduct"));
            }
        }

        public List<Model.Product> LoadItems(List<Model.Product> products)
        {
            var context = RoomDesignContext.Instance;

            // Linq om te zorgen dat de lijst gevuld wordt met de database content.
            Products = context.Products.ToList();
           
            // this.Products is de lijst met producten
            // context.Products is de table Products van de database 
            return Products;
        }

        public void SelectProduct(int id)
        {
            // Zet het geselecteerde product op basis van het gegeven ID
            var list = Products.Where(p => p.ProductId == id).ToList();
            Product product = list.FirstOrDefault();
            SelectedProduct = product;
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
        private void SubmitItem()
        {
            if (SaveProduct(Name, Price, Photo, Width, Length) != null)
            { // Als de parameters niet null zijn dan:
               RoomEditorPopupView Popup = new RoomEditorPopupView("Het product is opgeslagen");
               Popup.ShowDialog();
                Reload();
               // Popup dialog met "Het product is opgeslagen"
               return;
            }
        }

        public static Product SaveProduct(string naam, double price, string photo, int width, int length)
        {
            // als er geen foto wordt toegevoegd, dan krijgt foto een standaard waarde. 
            if (photo == null)
            {
                photo = "placeholder.png";
            }
            // Kamer opslaan
            Product product = new Product(naam, price, photo, width, length);
            // product = de waarde van de paramters
            var context = RoomDesignContext.Instance;
            product = context.Products.Add(product).Entity;

            
            // Zorgen dan het in de database komt
            try
            { // Proberen op te slaan, dan product returnen
                context.SaveChanges();
                return product;
            }
            catch (Exception e)
            { // Anders de exceptie opvangen en tonen, null returnen
                Console.WriteLine(e);
                return null;
            }
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

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}