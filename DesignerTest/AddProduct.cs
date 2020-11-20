using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Designer.Model;
using Designer.ViewModel;

namespace DesignerTest
{
    public static class AddProduct
    {
        private static readonly Product product1 = new Product("Bureaustoel", 51.40, "ernstig.png");
        private static readonly Product product2 = new Product("Tafel", 200, "ernstig.png", 30, 20);
        private static readonly Product product3 = new Product("Bureau", 140.40, "ernstig.png");

        [SetUp]
        public static void Setup()
        {
            // Producten in een lijst zetten als database om te testen.
            TestRoomDesignContext.Setup(
                products: new List<Product> {
                    product1, product2, product3
                }
            );
        }

        [Test]
        public static void TestSaveProduct()
        {
            Product productTest = ViewProductsViewModel.SaveProduct(product2.Name, product2.Price, product2.Photo, product2.Width, product2.Length);
            Assert.IsNotNull(productTest);
            var context = RoomDesignContext.Instance;
            List<Product> products = context.Products.ToList();


            Assert.AreEqual(products[products.Count-1], productTest);
        }
    }
}
