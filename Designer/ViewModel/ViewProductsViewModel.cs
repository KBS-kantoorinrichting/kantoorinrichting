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
            } 

        }





         public void FillDataBase()
         // Functie om snel de database te vullen met test producten.
         {
             using (var context =  RoomDesignContext.Instance)
             {
                 context.Products.Add(new Model.Product("Keukenstoel", 5.00, "nee"));
               
                 context.SaveChanges();
             }
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
