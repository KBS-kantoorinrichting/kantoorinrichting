using Designer.ViewModel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Models;
using ServicesTest;

namespace DesignerTest {
    public class ViewDesignTests : DatabaseTest {
        private static readonly Room Room = Room.FromDimensions("TestRoom", 250, 500);
        private static readonly Design Design = new Design("TestDesign", Room, new List<ProductPlacement>());
        private static readonly DesignEditor ViewModel = new DesignEditor(Design);
        private static readonly Product Product1 = new Product("Product1", 1, 5, 5);
        private static readonly Product Product2 = new Product("Product2", 2, 5, 5);
        private static readonly Product Product3 = new Product("Product3", 3, 5, 5);

        protected override List<Product> Products => new List<Product> {Product1, Product2, Product3};

        [Test]
        public void ViewDesign_SetDesign_ShouldSetDesign() {
            ViewModel.SetDesign(Design);

            Assert.IsNotNull(ViewModel.Design);
            Assert.IsNotNull(ViewModel.ProductPlacements);
        }

        [Test]
        public void ViewDesign_CatalogusMouseDown_ShouldSetSelectedProduct() {
            Product product = new Product {
                Id = 1,
                Name = "test"
            };
            ViewModel.Products = new List<Product>() {product};
            //ViewModel.SelectProduct(product.ProductId);
            //Assert.AreEqual(ViewModel.SelectedProduct.ProductId, product.ProductId);
        }

        [Test]
        public void ViewDesign_AddToOverview_ShouldBeAddedToOverview() {
            Product product = new Product {
                Id = 1,
                Name = "test"
            };
            ViewModel.AddToOverview(product);

            Product current = ViewModel.ProductOverview[0].Key;

            Assert.AreEqual(current, product);
        }

        [Test]
        public void ViewDesign_PlaceProduct_ShouldAddToProductPlacements() {
            //ViewDesign_MouseDown_ShouldSetSelectedProduct();
            // Zet de variable die bepaald of er iets geplaatst mag worden of niet
            ViewModel.ProductPlacements.Clear();
            ViewModel.AllowDrop = true;
            Product product = new Product {
                Id = 1,
                Name = "test"
            };
            ViewModel.PlaceProduct(product, 4, 20);
            Assert.AreEqual(4, ViewModel.ProductPlacements[0].X);
            Assert.AreEqual(20, ViewModel.ProductPlacements[0].Y);
            Assert.AreEqual(1, ViewModel.ProductPlacements[0].Product.Id);
        }

        [Test]
        public void ViewDesign_LoadProducts_Count() {
            List<Product> products = DesignEditor.LoadProducts();
            Assert.AreEqual(3, products.Count);
        }

        [Test]
        [TestCase(10, 10, ExpectedResult = true)]
        [TestCase(100, 100, ExpectedResult = true)]
        [TestCase(510, 500, ExpectedResult = false)]
        [TestCase(150, 270, ExpectedResult = true)]
        [TestCase(270, 150, ExpectedResult = false)]
        [TestCase(270, 270, ExpectedResult = false)]
        public bool ViewDesign_CheckRoomCollisions_ReturnBoolean(int x, int y) {
            Point point = new Point(x + Product1.Width / 2, y + Product1.Length / 2);

            return ViewModel.CheckRoomCollisions(point, Product1);
        }

        [Test]
        [TestCase(0,0,5,0, ExpectedResult = false)]
        [TestCase(0,0,6,0, ExpectedResult = true)]
        [TestCase(0,0,0,5, ExpectedResult = false)]
        [TestCase(0,0,0, 6, ExpectedResult = true)]
        public bool ViewDesign_CheckProductCollisions_ReturnBoolean(int x1, int y1, int x2, int y2)
        {
            ViewModel.ProductPlacements = new List<ProductPlacement>()
            {
                new ProductPlacement(x2,y2,Product1, Design)
            };
            return ViewModel.CheckProductCollisions(new ProductPlacement(x1, x2, Product1));
        }

        [Test]
        [TestCase(12.55)]
        [TestCase(78.95)]
        [TestCase(0.05)]
        [TestCase(1202.0)]
        public void ViewDesign_TotalPrice_ShouldReturnSum(double price) {
            ViewModel.SetDesign(Design);
            // TODO: needs fixing
            int increment = 5;
            for (int i = 0; i < increment; i++) {
                ViewModel.AddToOverview(
                    new Product() {
                        Id = i,
                        Price = price
                    }
                );
            }
            
            Assert.AreEqual(price * increment, ViewModel.TotalPrice);
        }
    }
}