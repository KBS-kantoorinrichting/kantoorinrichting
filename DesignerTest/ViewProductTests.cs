using Designer.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Designer.ViewModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NUnit.Framework;

namespace DesignerTest
{
    public class ViewProductTests
    {
        private static readonly Product product1 = new Product("Stoel", 51.40, "test");
        private static readonly Product product2 = new Product("Tafel", 200, "", 30, 20);
        private static readonly Product product3 = new Product("Bureau", 140.40, "");

        [SetUp]
        public void Setup()
        {
            // Producten in een lijst zetten als database om te testen.
            TestRoomDesignContext.Setup(
                products: new List<Product> {
                    product1, product2, product3
                }
            );
        }

        [Test]
        public void SelectProduct_Test()
        {
            ViewProductsViewModel Test = new ViewProductsViewModel();
            Test.SelectProduct(product1.ProductId);
            // Product1 selecteren

            Assert.IsNotNull(Test.SelectedProduct);
            // Kijken of product niet null is

            Assert.AreEqual(product1.Name, Test.SelectedProduct.Name);
            Assert.AreEqual(product1.Price, Test.SelectedProduct.Price);
            Assert.AreEqual(product1.Photo, Test.SelectedProduct.Photo);
            // Alle waarden van het geselecteerde product vergelijken met product1 met constructor zonder length en width

            Test.SelectProduct(product2.ProductId);
            Assert.AreEqual(product2.Name, Test.SelectedProduct.Name);
            Assert.AreEqual(product2.Price, Test.SelectedProduct.Price);
            Assert.AreEqual(product2.Photo, Test.SelectedProduct.Photo);
            Assert.AreEqual(product2.Length, Test.SelectedProduct.Length);
            Assert.AreEqual(product2.Width, Test.SelectedProduct.Width);
            // Alle waarden van het geselecteerde product vergelijken met product2 met length en width
        }
    }
}
