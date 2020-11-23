using Designer.ViewModel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Models;
using ServicesTest;

namespace DesignerTest
{
    public class ViewDesignTests : DatabaseTest {
        private static readonly Room Room = Room.FromDimensions("TestRoom", 1, 1);
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
            // Zet de variable die bepaald of er iets geplaatst mag worden of niet
            ViewModel.AllowDrop = true;
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

        [Test]
        [TestCase("0,0|500,0|500,500|0,500", ExpectedResult = 4)]
        [TestCase("0,0|250,0|250,250|500,250|500,500|0,500", ExpectedResult = 6)]
        public int ViewDesign_ConvertPosititionsToCoordinates_ShouldReturnList(string positions)
        {
            List<Coordinate> coordinates = ViewModel.ConvertPosititionsToCoordinates(positions);

            foreach(Coordinate coordinate in coordinates)
            {
                Assert.IsNotNull(coordinate.X);
                Assert.IsNotNull(coordinate.Y);
            }

            return coordinates.Count;
        }

        [Test]
        [TestCase(10, 10, ExpectedResult = true)]
        [TestCase(100, 100, ExpectedResult = true)]
        [TestCase(510, 500, ExpectedResult = false)]
        [TestCase(150, 270, ExpectedResult = true)]
        [TestCase(270, 150, ExpectedResult = false)]
        [TestCase(270, 270, ExpectedResult = true)]
        public bool ViewDesign_CheckRoomCollisions_ReturnBoolean(int x, int y)
        {
            List<Coordinate> coordinates = ViewModel.ConvertPosititionsToCoordinates("0,0|250,0|250,250|500,250|500,500|0,500");

            Point point = new Point(x, y);

            PointCollection points = new PointCollection();
            // Voeg de punten toe aan een punten collectie
            for (int i = 0; i < coordinates.Count; i++)
            {
                points.Add(new Point(coordinates[i].X, coordinates[i].Y));
            }

            return ViewModel.CheckRoomCollisions(points, point, Product1);
        }

        [Test]
        [TestCase(12.55)]
        [TestCase(78.95)]
        [TestCase(0.05)]
        [TestCase(1202.0)]
        public void ViewDesign_TotalPrice_ShouldReturnSum(double price)
        {
            ViewModel.SetDesign(Design);
            // TODO: needs fixing
            int increment = 5;
            for (int i = 0; i < increment; i++) {
                ViewModel.AddToOverview(new Product()
                {
                    Id = i,
                    Price = price
                });
            }
            Assert.AreEqual(price * increment, ViewModel.TotalPrice);
        }
    }
}
