using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Designer.ViewModel;
using Designer.Model;
using System.Linq;
using System.ComponentModel;

namespace DesignerTest
{
    public class CatalogTest
    {
        private static readonly Product product1 = new Product(1, "Bureaustoel", 51.40, "");
        private static readonly Product product2 = new Product(2, "Tuintafel", 200, "");
        private static readonly Product product3 = new Product(3, "Bureau", 140.40, "");
        
        [SetUp]
        public void Setup()
        {
        // Producten in een lijst zetten als database om te testen.
            TestRoomDesignContext.Setup(
                products:new List<Product> { 
                    product1, product2, product3
                }
            );
        }
        [Test]
        public void ViewProductsViewModel_DatabaseEqualsList()
        {
        // Items op het scherm vergelijken met items in de database.
            ViewProductsViewModel test = new ViewProductsViewModel();
            Assert.AreEqual(test.Products[0], product1);
            Assert.AreEqual(test.Products[1], product2);
            Assert.AreEqual(test.Products[2], product3);
        }

        [Test]
        public void GetName_NameIsCorrectName()
        {
        // De naam van de producten vergelijken met de naam van de producten in de database.
            ViewProductsViewModel test = new ViewProductsViewModel();
            Assert.AreEqual(product1.Name, test.GetName(1));
            Assert.AreEqual(product2.Name, test.GetName(2));
            Assert.AreEqual(product3.Name, test.GetName(3));
        }

        [Test]
        public void IdIsUnique()
        {
            ViewProductsViewModel test = new ViewProductsViewModel();
        // Eerst kijken of de ID's wel kloppen.
            Assert.AreEqual(product1.ProductId, test.Products[0].ProductId);
            Assert.AreEqual(product2.ProductId, test.Products[1].ProductId);
            Assert.AreEqual(product3.ProductId, test.Products[2].ProductId);

        // Daarna kijken of de ID's niet gelijk zijn.
            Assert.AreNotEqual(product1.ProductId, product2.ProductId);
            Assert.AreNotEqual(product2.ProductId, product3.ProductId);
            Assert.AreNotEqual(product1.ProductId, product3.ProductId);
        }

        [Test]
        public void GetPrice_PriceIsCorrectPrice()
        {
        // De prijs van de producten vergelijken met de prijs van de producten in de database.
            ViewProductsViewModel test = new ViewProductsViewModel();
            Assert.AreEqual(product1.Price, test.GetPrice(1));
            Assert.AreEqual(product2.Price, test.GetPrice(2));
            Assert.AreEqual(product3.Price, test.GetPrice(3));
        }

        [Test]
        public void GetPrice_PriceIsNotNull()
        {
        // Kijken of de prijs niet null is.
            ViewProductsViewModel test = new ViewProductsViewModel();
            Assert.IsNotNull(test.GetPrice(1));
            Assert.IsNotNull(test.GetPrice(2));
            Assert.IsNotNull(test.GetPrice(3));
        }

        [Test]
        public void GetPhoto_CorrectString()
        {
        // Kijken of de string overeenkomt met de string uit de database.
            ViewProductsViewModel test = new ViewProductsViewModel();
            Assert.AreEqual(product1.Photo, test.GetPhoto(1));
            Assert.AreEqual(product1.Photo, test.GetPhoto(2));
            Assert.AreEqual(product1.Photo, test.GetPhoto(3));
        }
    }
}
