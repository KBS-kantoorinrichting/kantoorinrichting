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



        public ViewProductsViewModel()
        {
            using (var context =  RoomDesignContext.Instance)
            {
                this.Products = context.Products.ToList();

            }

        }





       /* public void FillDataBase()
        {
            using (var context = new RoomDesignContext())
            {

                for (int i = 0; i < 100; i++)
                {
                    context.Products.Add(new Model.Product("bas", 5.00, "nee"));
                }
                context.SaveChanges();
            }
        }*/

    }
}
