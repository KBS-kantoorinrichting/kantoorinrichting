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

        public double GetPrice(string name)
        {

            var Price = from p in Products
                        where p.Name == name
                        select p.Price;

            return Convert.ToDouble(Price);

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
