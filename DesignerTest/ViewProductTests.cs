using System.Collections.Generic;
using Designer.ViewModel;
using Models;
using NUnit.Framework;
using ServicesTest;

namespace DesignerTest {
    public class ViewProductTests : DatabaseTest {
        private static readonly Product Product1 = new Product("Stoel", price: 51.40, photo: "test");
        private static readonly Product Product2 = new Product("Tafel", price: 200, photo: "", length: 30, width: 20);
        private static readonly Product Product3 = new Product("Bureau", price: 140.40, photo: "");

        protected override List<Product> Products => new List<Product> {Product1, Product2, Product3};

        [Test]
        public void SelectProduct_Test() {
            ViewProducts Test = new ViewProducts();
            Test.SelectProduct(Product1.Id);
            // Product1 selecteren

            Assert.IsNotNull(Test.SelectedProduct);
            // Kijken of product niet null is

            Assert.AreEqual(Product1.Name, Test.SelectedProduct.Name);
            Assert.AreEqual(Product1.Price, Test.SelectedProduct.Price);
            Assert.AreEqual(Product1.Photo, Test.SelectedProduct.Photo);
            // Alle waarden van het geselecteerde product vergelijken met product1 met constructor zonder length en width

            Test.SelectProduct(Product2.Id);
            Assert.AreEqual(Product2.Name, Test.SelectedProduct.Name);
            Assert.AreEqual(Product2.Price, Test.SelectedProduct.Price);
            Assert.AreEqual(Product2.Photo, Test.SelectedProduct.Photo);
            Assert.AreEqual(Product2.Length, Test.SelectedProduct.Length);
            Assert.AreEqual(Product2.Width, Test.SelectedProduct.Width);
            // Alle waarden van het geselecteerde product vergelijken met product2 met length en width
        }
    }
}