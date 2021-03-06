﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Designer.Utils;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Models;
using Services;

namespace Designer.ViewModel {
    public class ViewProducts : INotifyPropertyChanged {
        public ViewProducts() {
            // Tekenen van de catalogus 
            Reload();
            // Initialisatie van alle knoppen
            AddPhoto = new BasicCommand(SelectPhoto);
            Submit = new BasicCommand(SubmitItem);
            DeleteCommand = new ArgumentCommand<int>(Delete);
            EditCommand = new ArgumentCommand<int>(EditItem);
            AddCommand = new BasicCommand(AddItem);
            CancelAdd = new BasicCommand(CancelAddItem);
            SaveEdit = new BasicCommand(SubmitEditedItem);
            CancelEdit = new BasicCommand(CancelEditPopup);
            EditPhoto = new BasicCommand(SelectPhoto);
            //Controleert of het niet in een unit test wordt gedraaid
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                //Dit wordt gebruikt door de snackbar om feedback te geven op
                //verwijderen, toevoegen en aanpassen van producten
                MessageQueue = new SnackbarMessageQueue();
        }

        public string Name { get; set; }
        public double Price { get; set; }
        public string Photo { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
        public bool HasPerson { get; set; }

        public BasicCommand Submit { get; set; }
        public ArgumentCommand<int> DeleteCommand { get; set; }
        public BasicCommand AddPhoto { get; set; }

        public ArgumentCommand<int> EditCommand { get; set; }
        public BasicCommand SaveEdit { get; set; }
        public BasicCommand CancelEdit { get; set; }
        public BasicCommand EditPhoto { get; set; }
        public BasicCommand AddCommand { get; set; }
        public BasicCommand CancelAdd { get; set; }
        public Product SelectedProduct { get; set; }

        public Product EditedItem { get; set; }

        public bool IsEditedRead => EditedItem == null;
        public bool IsAdding { get; set; }
        public bool IsEditing { get; set; }

        public List<Product> Products { get; set; }
        // Property van een lijst om de informatie vanuit de database op te slaan.

        public SnackbarMessageQueue MessageQueue { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

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
                if (IsEditedRead) {
                    //openFileDialog.Filter = "Image files (*.png;*.jpeg;*.gif)|*.png;*.jpeg;*.gif|All files (*.*)|*.*";
                    Photo = openFileDialog.FileName.Split(@"\").Last();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Photo"));
                    // De foto wordt veranderd in de applicatie
                } else {
                    //openFileDialog.Filter = "Image files (*.png;*.jpeg;*.gif)|*.png;*.jpeg;*.gif|All files (*.*)|*.*";
                    EditedItem.Photo = openFileDialog.FileName.Split(@"\").Last();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
                    // De foto wordt veranderd in de applicatie
                }
            }
        }

        public void Delete(int id) // Verwijderd geselecteerde item uit database 
        {
            SelectProduct(id);
            if (SelectedProduct == null || ProductService.Instance.Count() == 0) return;

            ProductService.Instance.Delete(SelectedProduct);
            MessageQueue.Enqueue("Het product is verwijderd");
            Reload();
        }

        public void AddItem() {
            IsAdding = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void CancelAddItem() {
            IsAdding = false;
            ResetFields();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void ResetFields() {
            Name = "";
            Price = 0;
            Photo = "";
            Width = 0;
            Length = 0;
            HasPerson = false;
        }

        public void EditItem(int id) // Maakt de geselecteerde item editbaar
        {
            SelectProduct(id);
            EditedItem = SelectedProduct;
            IsEditing = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        public void CancelEditPopup() {
            IsEditing = false;
            OnPropertyChanged();
            EditedItem = null;
            SelectedProduct = null;
        }

        public void SubmitEditedItem() // vervangt het geselecteerde item met de bijgewerkte item in de database
        {
            IsEditing = false;
            ProductService.Instance.Update(EditedItem);
            //Laat bericht zien in snackbar
            MessageQueue.Enqueue("Het product is aangepast");
            Reload();
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
            if (SaveProduct(Name, Price, Photo, Width, Length, HasPerson) != null) {
                // Als de parameters niet null zijn dan:
                // Laat bericht zien in de snackbar
                MessageQueue.Enqueue("Het product is toegevoegd");
                IsAdding = false;
                Reload();
            }
        }

        public static Product SaveProduct(
            string naam,
            double? price,
            string photo,
            int width,
            int length,
            bool hasPerson
        ) {
            // als er geen foto wordt toegevoegd, dan krijgt foto een standaard waarde
            if (photo == null) photo = "placeholder.png";

            // Kamer opslaan
            Product product = new Product(
                naam, price: price, photo: photo, width: width, length: length, hasPerson: hasPerson
            );

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