using Designer.Model;
using Designer.ViewModel;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DesignerTest
{
    public class ViewDesignTests
    {
        private static readonly Room Room = new Room( "TestRoom", 1, 1);
        private static readonly Design Design = new Design("TestDesign", Room, new List<ProductPlacement>());
        private static readonly ViewDesignViewModel ViewModel = new ViewDesignViewModel(Design);

       /* [Test]
        public void ViewDesign_MouseDown_ShouldSetSelectedProduct()
        {
            Product product = new Product
            {
                ProductId = 1,
                Name = "test"
            };
            ViewModel.Products = new List<Product>(){product};
            ViewModel.SelectProduct(product.ProductId);
            Assert.AreEqual(ViewModel.SelectedProduct.ProductId, product.ProductId);
        }*/

/*
        [Test]
        public void ViewDesign_PlaceProduct_ShouldAddToProductPlacements()
        {
            ViewDesign_MouseDown_ShouldSetSelectedProduct();
            ViewModel.PlaceProduct(4,20);
            Assert.AreEqual(ViewModel.ProductPlacements[0].X, 4);
            Assert.AreEqual(ViewModel.ProductPlacements[0].Y, 20);
            Assert.AreEqual(ViewModel.ProductPlacements[0].Product.ProductId, 1);
        }*/
    }
}
