using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Designer.Other;
using System.Windows.Controls;
using Designer.View;
using Microsoft.Win32;
using System.Diagnostics;
using Models;
using Services;

namespace Designer.ViewModel {
    public class ViewProductsViewModel : INotifyPropertyChanged {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Photo { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
        public bool HasPerson { get; set; }


        public BasicCommand Submit { get; set; }
        public BasicCommand DeleteCommand { get; set; }
        public BasicCommand AddPhoto { get; set; }

        public BasicCommand EditCommand { get; set; }
        public BasicCommand SaveEdit { get; set; }
        public BasicCommand EditPhoto { get; set; }

        public ArgumentCommand<MouseButtonEventArgs> MouseDownCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public Product SelectedProduct { get; set; }

        public Product EditedItem { get; set; }

        public string IsEditedRead
        {
            get
            {
                return (EditedItem == null).ToString();
            }
            set { IsEditedRead = value; }
        }

        public string IsEditedVis
        {
            get
            {
                return EditedItem == null ? "Hidden" : "Visible";
            }
            set { IsEditedVis = value; }
        }

        public string HasPersonSelectedTrue
        {
            get
            {
                if(SelectedProduct != null)
                {
                    return (SelectedProduct.HasPerson == true).ToString();
                }
                else
                {
                    return false.ToString();
                }
                
            }
            set { HasPersonSelectedTrue = value; }
        }
        public string HasPersonSelectedFalse
        {
            get
            {
                if(SelectedProduct != null)
                {
                    return (SelectedProduct.HasPerson == false).ToString();
                }
                else
                {
                    return false.ToString();
                }

            }
            set { HasPersonSelectedFalse = value; }
        }

        public string ItemIsSelected
        {
            get
            {
                return SelectedProduct == null ? "Hidden" : "Visible";
            }
            set { ItemIsSelected = value; }
          
        }

        public List<Product> Products { get; set; }
        // Property van een lijst om de informatie vanuit de database op te slaan.

        public ViewProductsViewModel() {

            // Tekenen van de catalogus 
            Reload();
            // Initialisatie van het MouseDownCommand
            MouseDownCommand = new ArgumentCommand<MouseButtonEventArgs>(e => MouseDown(e.OriginalSource, e));
            // Initialisatie van alle knoppen
            AddPhoto = new BasicCommand(SelectPhoto);
            Submit = new BasicCommand(SubmitItem);
            DeleteCommand = new BasicCommand(Delete);
            EditCommand = new BasicCommand(EditItem);
            SaveEdit = new BasicCommand(SubmitEditedItem);
            EditPhoto = new BasicCommand(SelectPhoto);
        }

        public void Reload() { // Reload de items zodat de juiste te zien zijn
            Products = LoadItems();
            OnPropertyChanged();
        }

        public void SelectPhoto() {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"{Environment.CurrentDirectory}\Resources\Images\";
            Debug.WriteLine(openFileDialog.InitialDirectory);
            if (openFileDialog.ShowDialog() == true)
                // Als deze open is dan:
            {
                if (IsEditedRead == "True")
                {
                    //openFileDialog.Filter = "Image files (*.png;*.jpeg;*.gif)|*.png;*.jpeg;*.gif|All files (*.*)|*.*";
                    Photo = openFileDialog.FileName.Split(@"\").Last();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Photo"));
                    // De foto wordt veranderd in de applicatie
                }
                else
                {
                    //openFileDialog.Filter = "Image files (*.png;*.jpeg;*.gif)|*.png;*.jpeg;*.gif|All files (*.*)|*.*";
                    EditedItem.Photo = openFileDialog.FileName.Split(@"\").Last();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
                    // De foto wordt veranderd in de applicatie

                }

            }
        }

        public void Delete() // Verwijderd geselecteerde item uit database 
        { 
            if (SelectedProduct == null || ProductService.Instance.Count() == 0)
            {
                return;
            }
            ProductService.Instance.Delete(SelectedProduct);
            Reload();
        }

        public void EditItem() // Maakt de geselecteerde item editbaar
        {
            EditedItem = SelectedProduct;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void SubmitEditedItem() // vervangt het geselecteerde item met de bijgewerkte item in de database
        {
            ProductService.Instance.Update(EditedItem);
            Reload();
        }

        public void MouseDown(object sender, MouseButtonEventArgs e) { // Wat er gebeurt als de muisknop ingedrukt wordt
           
            // Linker muisknop moet ingdrukt zijn
            if (e.LeftButton == MouseButtonState.Pressed) {
                if (sender.GetType() != typeof(Image)) return;
                var obj = (Product) ((Image) sender).DataContext;
                //SelectedProduct = obj;
                SelectProduct(obj.Id);
                EditedItem = null;
                Reload();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));



            }
        }

        public List<Product> LoadItems() {
            // Linq om te zorgen dat de lijst gevuld wordt met de database content
            Products = ProductService.Instance.GetAll();

            // this.Products is de lijst met producten
            // context.Products is de table Products van de database 
            return Products;
        }

        public void SelectProduct(int id) {
            // Zet het geselecteerde product op basis van het gegeven ID
            Product product = Products.FirstOrDefault(p => p.Id == id);
            SelectedProduct = product;
        }

        #region Database_Fill_Code

        // Functie om snel de database te vullen met test producten.
        public void FillDataBase() {
            ProductService.Instance.Save(new Product("Bureaustoel", price: 51.30, photo: ""));
            ProductService.Instance.Save(new Product("Tuintafel", price: 200.00, photo: ""));
            ProductService.Instance.Save(new Product("Bureau", price: 140.40, photo: ""));
            ProductService.Instance.Save(new Product("Lamp", price: 30.60, photo: ""));
            ProductService.Instance.Save(new Product("Kleed", price: 80.00, photo: ""));
            ProductService.Instance.Save(new Product("Koffieapparaat", price: 600, photo: ""));
            ProductService.Instance.Save(new Product("Willem", price: 5.99, photo: ""));
        }

        #endregion

        private void SubmitItem() {
            if (SaveProduct(Name, Price, Photo, Width, Length, HasPerson) != null) { // Als de parameters niet null zijn dan:
                GeneralPopup popup = new GeneralPopup("Het product is opgeslagen");
                popup.ShowDialog();
                Reload();
                // Popup dialog met "Het product is opgeslagen"
            }
        }

        public static Product SaveProduct(string naam, double? price, string photo, int width, int length, bool hasPerson) {
            // als er geen foto wordt toegevoegd, dan krijgt foto een standaard waarde
            if (photo == null) {
                photo = "placeholder.png";
            }

            // Kamer opslaan
            Product product = new Product(naam, price: price, photo: photo, width: width, length: length, hasPerson: hasPerson);

            // Zorgen dan het in de database komt
            try { // Proberen op te slaan, dan product returnen
                return ProductService.Instance.Save(product);
            } catch (Exception e) { // Anders de exceptie opvangen en tonen, null returnen
                Console.WriteLine(e);
                return null;
            }
        }

        public void EmptyDataBase()
            // Functie om de database te legen   
        {
            ProductService.Instance.DeleteAll(ProductService.Instance.GetAll());
        }

        protected virtual void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}