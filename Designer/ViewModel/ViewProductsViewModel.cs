using Designer.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Designer.ViewModel
{
    public class ViewProductsViewModel
    {


        public event PropertyChangedEventHandler PropertyChanged;

        public List<Model.Product> Products { get; set; }
        // Property van een lijst om de informatie vanuit de database op te slaan.
        

        public ViewProductsViewModel()
        {
            using (var context =  RoomDesignContext.Instance)
            // Linq om te zorgen dat de lijst gevuld wordt met de database content.
            {
                
                this.Products = context.Products.ToList();
                // this.Products is de lijst met producten
                // context.Products is de table Products van de database 
                
            }
        }



        #region Database_Fill_Code

        public void FillDataBase()
         // Functie om snel de database te vullen met test producten.
         {
             using (var context =  RoomDesignContext.Instance)
             {
                context.Products.Add(new Model.Product(1, "Bureaustoel", 51.30, ""));
                context.Products.Add(new Model.Product(2, "Tuintafel", 200.00, ""));
                context.Products.Add(new Model.Product(3, "Bureau", 140.40, ""));
                context.Products.Add(new Model.Product(4, "Lamp", 30.60, ""));
                context.Products.Add(new Model.Product(5, "Kleed", 80.00, ""));
                context.Products.Add(new Model.Product(6, "Koffieapparaat", 600, ""));
                context.Products.Add(new Model.Product(7, "Willem", 5.99, ""));

                context.SaveChanges();
             }
         }

        #endregion

        public string? GetName(int id)
        {
        // Functie om naam op te halen met het ID
            var Name = from n in Products
                       where n.ProductId == id
                       select n.Name;

            return Name.First();
        }
        public double? GetPrice(int id) 
        // Functie om de prijs op te halen van de producten
        {

            var Price = from p in Products
                        where p.ProductId == id
                        select p.Price;

            return Price.First();
        }

        public string? GetPhoto(int id) 
        {
        // Functie om de string van waar de foto van de producten staat op te halen
            var Photo = from p in Products
                        where p.ProductId == id
                        select p.Photo;

            return Photo.First();
        }
        public void EmptyDataBase()
        // Functie om de database te legen.    
        {
            using (var context = RoomDesignContext.Instance)
            {
                for (int i = 0; i < Products.Count; i++)
                {
                    context.Products.Remove(Products[i]);
                }

                 context.SaveChanges();
            }

        }

    }
}
