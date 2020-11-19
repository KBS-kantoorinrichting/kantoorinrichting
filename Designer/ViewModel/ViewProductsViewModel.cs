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
namespace Designer.ViewModel
{
    public class ViewProductsViewModel : INotifyPropertyChanged {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Photo { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
        public string ImageAdress { get; set; }
        public BasicCommand Submit { get; set; }
        public BasicCommand AddPhoto { get; set; }

        public BasicCommand ReloadCommand { get; set; }
        public ArgumentCommand<MouseButtonEventArgs> MouseDownCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public Product SelectedProduct { get; set; }


        public List<Model.Product> Products { get; set; }
        // Property van een lijst om de informatie vanuit de database op te slaan.

        public ViewProductsViewModel() {
            MouseDownCommand = new ArgumentCommand<MouseButtonEventArgs>(e => CatalogusMouseDown(e.OriginalSource, e));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedProduct"));
            Submit = new BasicCommand(SubmitItem);
            AddPhoto = new BasicCommand(SelectPhoto);
            Products = LoadItems(Products);
            ReloadCommand = new BasicCommand(Reload);

        }

        public void Reload()
        {
            Products = LoadItems(Products);
            OnPropertyChanged();
        }

        public void SelectPhoto()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                openFileDialog.InitialDirectory = @"C:\Users\bashe\source\repos\kantoorinrichting\Designer\Resources\Images";
                openFileDialog.Filter = "Image files (*.png;*.jpeg)|*.png;*.jpeg|All files (*.*)|*.*";
                ImageAdress = openFileDialog.FileName.Substring(openFileDialog.InitialDirectory.Length+1);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ImageAdress"));
                
            }



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
            if (SaveProduct(Name, Price, Photo, Width, Length, ImageAdress) != null)
            {
               RoomEditorPopupView Popup = new RoomEditorPopupView("Het product is opgeslagen");
                Popup.ShowDialog();
               return;
            }
        }

        public static Product SaveProduct(string naam, double price, string photo, int width, int length, string ImageAdress)
        {
            // Kamer opslaan
            Product product = new Product(naam, price, photo, width, length, ImageAdress);
            var context = RoomDesignContext.Instance;
            product = context.Products.Add(product).Entity;
            try
            {
                context.SaveChanges();
                return product;
            }
            catch (Exception e)
            {
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