using Designer.Model;
using Designer.ViewModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace DesignerTest
{
    public class ViewDesignTests
    {
        private static readonly Room Room = new Room(1, "TestRoom", 1, 1);
        private static readonly Design Design = new Design("TestDesign", Room, new List<ProductPlacement>());
        private static readonly ViewDesignViewModel ViewModel = new ViewDesignViewModel();

        [Test]
        public void ViewDesign_MouseDown_ShouldSetSelectedProduct()
        {
            Product product = new Product
            {
                ProductId = 1,
                Name = "test"
            };
            
            ViewModel.SelectProduct(product.ProductId);

            Assert.AreEqual(ViewModel.SelectedProduct.ProductId, product.ProductId);
        }
    }
}
