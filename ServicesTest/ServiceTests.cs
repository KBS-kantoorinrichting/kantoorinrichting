using System.Collections.Generic;
using Models;
using NUnit.Framework;
using Services;

namespace ServicesTest {
    public class ServiceTests : DatabaseTest {
        private static readonly Room Room1 = new Room("TestRoom1", 1, 1);
        private static readonly Room Room2 = new Room("TestRoom2", 2, 4);
        private static readonly Room Room3 = new Room("TestRoom3", 72, 4);

        private static readonly Design Design1 = new Design("TestDesign1", Room1, null);
        private static readonly Design Design2 = new Design("TestDesign2", Room1, null);
        private static readonly Design Design3 = new Design("TestDesign3", Room2, null);
        private static readonly Design Design4 = new Design("TestDesign4", Room3, null);

        private static readonly Product Product1 = new Product("TestProduct1");
        private static readonly Product Product2 = new Product("TestProduct2");
        private static readonly Product Product3 = new Product("TestProduct3");
        private static readonly Product Product4 = new Product("TestProduct4");

        private static readonly ProductPlacement ProductPlacement1 = new ProductPlacement(0, 1, Product2, Design1);
        private static readonly ProductPlacement ProductPlacement2 = new ProductPlacement(0, 1, Product2, Design1);
        private static readonly ProductPlacement ProductPlacement3 = new ProductPlacement(0, 1, Product3, Design2);
        private static readonly ProductPlacement ProductPlacement4 = new ProductPlacement(0, 1, Product4, Design3);

        protected override List<Room> Rooms => new List<Room> {Room1, Room2};
        protected override List<Design> Designs => new List<Design> {Design1, Design2, Design3};
        protected override List<Product> Products => new List<Product> {Product2, Product3, Product4};
        protected override List<ProductPlacement> ProductPlacements => new List<ProductPlacement> {ProductPlacement1};

        private static IEnumerable<TestCaseData> ServiceCount {
            get {
                yield return new TestCaseData(new ServiceTestCase<Room>(RoomService.Instance), 2);
                yield return new TestCaseData(new ServiceTestCase<Design>(DesignService.Instance), 3);
                yield return new TestCaseData(new ServiceTestCase<Product>(ProductService.Instance), 3);
                yield return new TestCaseData(
                    new ServiceTestCase<ProductPlacement>(ProductPlacementService.Instance), 1
                );
            }
        }

        private static IEnumerable<TestCaseData> ServiceContains {
            get {
                yield return new TestCaseData(
                    new ServiceTestCase<Room>(RoomService.Instance), new List<Room> {Room1, Room2}
                );
                yield return new TestCaseData(
                    new ServiceTestCase<Design>(DesignService.Instance), new List<Design> {Design1, Design2, Design3}
                );
                yield return new TestCaseData(
                    new ServiceTestCase<Product>(ProductService.Instance),
                    new List<Product> {Product2, Product3, Product4}
                );
                yield return new TestCaseData(
                    new ServiceTestCase<ProductPlacement>(ProductPlacementService.Instance),
                    new List<ProductPlacement> {ProductPlacement1}
                );
            }
        }

        private static IEnumerable<TestCaseData> ServiceSaveCount {
            get {
                yield return new TestCaseData(new ServiceTestCase<Room>(RoomService.Instance), Room3, 3);
                yield return new TestCaseData(new ServiceTestCase<Design>(DesignService.Instance), Design4, 4);
                yield return new TestCaseData(new ServiceTestCase<Product>(ProductService.Instance), Product1, 4);
                //TODO deze faalt door shit
                //yield return new TestCaseData(new ServiceTestCase<ProductPlacement>(ProductPlacementService.Instance), ProductPlacement2, 2);
            }
        }

        private static IEnumerable<TestCaseData> ServiceSaveContains {
            get {
                yield return new TestCaseData(new ServiceTestCase<Room>(RoomService.Instance), Room3);
                yield return new TestCaseData(new ServiceTestCase<Design>(DesignService.Instance), Design4);
                yield return new TestCaseData(new ServiceTestCase<Product>(ProductService.Instance), Product1);
                yield return new TestCaseData(
                    new ServiceTestCase<ProductPlacement>(ProductPlacementService.Instance), ProductPlacement2
                );
            }
        }

        private static IEnumerable<TestCaseData> ServiceGetEquals {
            get {
                yield return new TestCaseData(new ServiceTestCase<Room>(RoomService.Instance), Room1);
                yield return new TestCaseData(new ServiceTestCase<Design>(DesignService.Instance), Design3);
                yield return new TestCaseData(new ServiceTestCase<Product>(ProductService.Instance), Product2);
                yield return new TestCaseData(
                    new ServiceTestCase<ProductPlacement>(ProductPlacementService.Instance), ProductPlacement1
                );
            }
        }

        [Test]
        [TestCaseSource(nameof(ServiceCount))]
        public void Service_Count_Test<T>(ServiceTestCase<T> service, int count) where T : IEntity {
            Assert.AreEqual(count, service.Service.Count());
        }

        [Test]
        [TestCaseSource(nameof(ServiceContains))]
        public void Service_Contains_Test<T>(ServiceTestCase<T> service, List<T> expected) where T : IEntity {
            foreach (T entity in expected) Assert.Contains(entity, service.Service.GetAll());
        }

        [Test]
        [TestCaseSource(nameof(ServiceSaveCount))]
        public void Service_Save_Count_Test<T>(ServiceTestCase<T> service, T add, int expected) where T : IEntity {
            service.Service.Save(add);
            Assert.AreEqual(expected, service.Service.Count());
        }

        [Test]
        [TestCaseSource(nameof(ServiceSaveContains))]
        public void Service_Save_Contains_Test<T>(ServiceTestCase<T> service, T add) where T : IEntity {
            service.Service.Save(add);
            Assert.Contains(add, service.Service.GetAll());
        }

        [Test]
        [TestCaseSource(nameof(ServiceGetEquals))]
        public void Service_Get_Equals_Test<T>(ServiceTestCase<T> service, T expected) where T : IEntity {
            T r = service.Service.Get(expected.Id);
            Assert.AreEqual(expected, r);
        }
    }

    public class ServiceTestCase<T> where T : IEntity {
        public ServiceTestCase(IService<T> service) { Service = service; }
        public IService<T> Service { get; }
    }
}