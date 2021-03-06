﻿using Designer.ViewModel;
using Models;
using NUnit.Framework;
using Services;
using ServicesTest;

namespace DesignerTest {
    public class AddProduct : DatabaseTest {
        private static readonly Product TestProduct = new Product(
            "Tafel", price: 200, photo: "ernstig.png", length: 30, width: 20
        );

        [Test]
        public void TestSaveProduct() {
            Product productTest = ViewProducts.SaveProduct(
                TestProduct.Name, TestProduct.Price, TestProduct.Photo, TestProduct.Width, TestProduct.Length,
                TestProduct.HasPerson
            );

            Assert.IsNotNull(productTest);
            Assert.AreEqual(1, ProductService.Instance.Count());
            Assert.Contains(productTest, ProductService.Instance.GetAll());
        }
    }
}