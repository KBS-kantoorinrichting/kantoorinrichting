using System.Collections.Generic;
using NUnit.Framework;
using Designer.ViewModel;
using Models;
using ServicesTest;

namespace DesignerTest {
    public class CatalogTests : DatabaseTest {
        private static readonly Product Product1 = new Product("Bureaustoel yaksva", price: 51.40, photo: "");
        private static readonly Product Product2 = new Product("Tuintafel", price: 200, photo: "");
        private static readonly Product Product3 = new Product("Bureau", price: 140.40, photo: "");

        protected override List<Product> Products => new List<Product> {Product1, Product2, Product3};

        [Test]
        public void ViewProductsViewModel_DatabaseEqualsList() {
            // Items op het scherm vergelijken met items in de database.
            ViewProducts test = new ViewProducts();
            Assert.AreEqual(test.Products[0], Product1);
            Assert.AreEqual(test.Products[1], Product2);
            Assert.AreEqual(test.Products[2], Product3);
        }

        [Test]
        public void GetName_NameIsCorrectName() {
            // De naam van de producten vergelijken met de naam van de producten in de database.
            ViewProducts test = new ViewProducts();
            Assert.AreEqual(Product1.Name, test.Products[0].Name);
            Assert.AreEqual(Product2.Name, test.Products[1].Name);
            Assert.AreEqual(Product3.Name, test.Products[2].Name);
        }

        [Test]
        public void GetPrice_PriceIsCorrectPrice() {
            // De prijs van de producten vergelijken met de prijs van de producten in de database.
            ViewProducts test = new ViewProducts();
            Assert.AreEqual(Product1.Price, test.Products[0].Price);
            Assert.AreEqual(Product2.Price, test.Products[1].Price);
            Assert.AreEqual(Product3.Price, test.Products[2].Price);
        }

        [Test]
        public void GetPrice_PriceIsNotNull() {
            // Kijken of de prijs niet null is.
            ViewProducts test = new ViewProducts();
            Assert.IsNotNull(test.Products[0].Price);
            Assert.IsNotNull(test.Products[1].Price);
            Assert.IsNotNull(test.Products[2].Price);
        }

        [Test]
        public void GetPhoto_CorrectString() {
            // Kijken of de string overeenkomt met de string uit de database.
            ViewProducts test = new ViewProducts();
            Assert.AreEqual(Product1.Photo, test.Products[0].Photo);
            Assert.AreEqual(Product2.Photo, test.Products[1].Photo);
            Assert.AreEqual(Product3.Photo, test.Products[2].Photo);
        }
    }
}