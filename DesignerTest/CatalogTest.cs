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
        [SetUp]

        [Test]
        public void ViewProductsViewModel_DatabaseEqualsList()
        {
            using var context = RoomDesignContext.Instance;
            ViewProductsViewModel test = new ViewProductsViewModel();
            foreach (var product in test.Products)
            {
                Assert.AreEqual(test.Products, context.Products.ToList());
            }
        }

        public void ViewProductsViewModel_PriceIsNotNull()
        {
            using var context = RoomDesignContext.Instance;
            Assert.IsNotNull(context.Products.Find());
        }
    }
}
