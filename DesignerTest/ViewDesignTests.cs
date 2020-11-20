using Designer.ViewModel;
using NUnit.Framework;
using System.Collections.Generic;
using Models;
using ServicesTest;

namespace DesignerTest
{
    public class ViewDesignTests : DatabaseTest {
        private static readonly Room Room = new Room.FromDimensions("TestRoom", 1, 1);
        private static readonly Design Design = new Design("TestDesign", Room, new List<ProductPlacement>());
        private static readonly ViewDesignViewModel ViewModel = new ViewDesignViewModel(Design);
        private static readonly Product Product1 = new Product() { Id = 1, Name = "Product1" };
        private static readonly Product Product2 = new Product() { Id = 2, Name = "Product2" };
        private static readonly Product Product3 = new Product() { Id = 3, Name = "Product3" };

        protected override List<Product> Products => new List<Product> {Product1, Product2, Product3};

        [Test]
        public void ViewDesign_SetDesign_ShouldSetDesign()
        {
            ViewModel.SetDesign(Design);

            Assert.IsNotNull(ViewModel.Design);
            Assert.IsNotNull(ViewModel.ProductPlacements);
        }

        [Test]
        public void ViewDesign_CatalogusMouseDown_ShouldSetSelectedProduct()
        {
            Product product = new Product
            {
                Id = 1,
                Name = "test"
            };
            ViewModel.Products = new List<Product>(){product};
            //ViewModel.SelectProduct(product.ProductId);
            //Assert.AreEqual(ViewModel.SelectedProduct.ProductId, product.ProductId);
        }

        [Test]
        public void ViewDesign_AddToOverview_ShouldBeAddedToOverview()
        {
            Product product = new Product
            {
                Id = 1,
                Name = "test"
            };
            ViewModel.AddToOverview(product);

            Product current = ViewModel.ProductOverview[0].Key;

            Assert.AreEqual(current, product);
        }

        [Test]
        public void ViewDesign_PlaceProduct_ShouldAddToProductPlacements()
        {
            //ViewDesign_MouseDown_ShouldSetSelectedProduct();
            Product product = new Product
            {
                Id = 1,
                Name = "test"
            };
            ViewModel.PlaceProduct(product, 4,20);
            Assert.AreEqual(ViewModel.ProductPlacements[0].X, 4);
            Assert.AreEqual(ViewModel.ProductPlacements[0].Y, 20);
            Assert.AreEqual(ViewModel.ProductPlacements[0].Product.Id, 1);
        }

        [Test]
        public void ViewDesign_LoadProducts_Count()
        {
            List<Product> products = ViewDesignViewModel.LoadProducts();
            Assert.AreEqual(3, products.Count);
        }
    }
}
