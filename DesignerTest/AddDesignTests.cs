using System.Collections.Generic;
using System.Linq;
using Designer.ViewModel;
using Models;
using NUnit.Framework;
using Repositories;
using RepositoriesTest;

namespace DesignerTest {
    public class AddDesignTestsStaticMethods {
        private static readonly Room Room1 = new Room("TestRoom1", 1, 1);
        private static readonly Room Room2 = new Room("TestRoom2", 2, 4);
        private static readonly Room Room3 = new Room("TestRoom3", 2, 5);

        [SetUp]
        public void Setup() {
            TestRepository.Setup(
                new List<Room> {
                    Room1, Room2, Room3
                }
            );
        }

        [Test]
        public void LoadRooms_Count() {
            List<Room> rooms = AddDesignModel.LoadRooms();
            Assert.AreEqual(3, rooms.Count);
        }

        [Test]
        public void LoadRooms_Contains() {
            List<Room> rooms = AddDesignModel.LoadRooms();
            Assert.Contains(Room1, rooms);
            Assert.Contains(Room2, rooms);
            Assert.Contains(Room3, rooms);
        }

        [Test]
        public void CreateDesign_Room_Name() {
            CreateDesign("design1", Room1);
            CreateDesign("design2", Room2);
            CreateDesign("design3", Room3);
        }

        private void CreateDesign(string name, Room room) {
            Design design = AddDesignModel.CreateDesign(name, room);
            Assert.AreEqual(name, design.Name);
            Assert.AreEqual(room.Name, design.Room.Name);
            Assert.AreEqual(room.RoomId, design.Room.RoomId);
            Assert.AreEqual(room.Width, design.Room.Width);
            Assert.AreEqual(room.Length, design.Room.Length);
            Assert.IsEmpty(design.ProductPlacements);
        }

        [Test]
        public void SaveDesign_SameAsReturn() {
            Design design = new Design("design4", Room1, new List<ProductPlacement>());
            Design returnedDesign = AddDesignModel.SaveDesign(design);

            Assert.AreEqual(design.DesignId, returnedDesign.DesignId);
            Assert.AreEqual(design.Name, returnedDesign.Name);
            Assert.AreEqual(design.RoomId, returnedDesign.RoomId);
            Assert.AreEqual(design.Room, returnedDesign.Room);
            Assert.AreEqual(design.ProductPlacements, returnedDesign.ProductPlacements);
        }

        [Test]
        public void SaveDesign_AddToDb_1() {
            Design design = new Design("design4", Room1, new List<ProductPlacement>());
            AddDesignModel.SaveDesign(design);

            int count = RoomDesignContext.Instance.Designs.Count();

            Assert.AreEqual(1, count);
        }

        [Test]
        [TestCase(10)]
        [TestCase(0)]
        [TestCase(5)]
        [TestCase(100)]
        public void SaveDesign_AddToDb_Multiple(int times) {
            for (int i = 0; i < times; i++) {
                Design design = new Design($"design{times}", Room1, new List<ProductPlacement>());
                AddDesignModel.SaveDesign(design);
            }

            int count = RoomDesignContext.Instance.Designs.Count();

            Assert.AreEqual(times, count);
        }

        [Test]
        public void SaveDesign_SameAsDb() {
            Design design = new Design("design4", Room1, new List<ProductPlacement>());
            AddDesignModel.SaveDesign(design);

            Design dbDesign = RoomDesignContext.Instance.Designs.First();
            Assert.AreEqual(design.DesignId, dbDesign.DesignId);
            Assert.AreEqual(design.Name, dbDesign.Name);
            Assert.AreEqual(design.RoomId, dbDesign.RoomId);
            Assert.AreEqual(design.Room, dbDesign.Room);
            Assert.AreEqual(design.ProductPlacements, dbDesign.ProductPlacements);
        }
    }

    public class AddDesignTestsInstace {
        private static readonly Room Room1 = new Room("TestRoom1", 1, 1);
        private static readonly Room Room2 = new Room("TestRoom2", 2, 4);
        private static readonly Room Room3 = new Room("TestRoom3", 2, 5);
        private static readonly string TestName = "TestDesign";

        private AddDesignModel _designModel;

        [SetUp]
        public void Setup() {
            TestRepository.Setup(
                new List<Room> {
                    Room1, Room2, Room3
                }
            );
            _designModel = new AddDesignModel();
        }

        [Test]
        public void AddDesign_NeedsAll() {
            _designModel.AddDesign();

            int count = RoomDesignContext.Instance.Designs.Count();
            Assert.AreEqual(0, count);
        }

        [Test]
        public void AddDesign_IsAdded() {
            _designModel.Name = TestName;
            _designModel.Selected = Room1;
            
            _designModel.AddDesign();

            Design design = RoomDesignContext.Instance.Designs.First();
            Assert.NotNull(design);
            Assert.AreEqual(TestName, design.Name);
            Assert.AreEqual(Room1, design.Room);
            Assert.IsEmpty(design.ProductPlacements);
        }

        [Test]
        public void AddDesign_IsCorrect() {
            _designModel.Name = TestName;
            _designModel.Selected = Room1;
            
            _designModel.AddDesign();

            int count = RoomDesignContext.Instance.Designs.Count();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void AddDesign_TriggersEvent() {
            _designModel.Name = TestName;
            _designModel.Selected = Room1;

            bool triggered = false;

            _designModel.DesignAdded += (sender, args) => triggered = true;
            _designModel.AddDesign();

            Assert.IsTrue(triggered);
        }

        [Test]
        public void DesignAdded_DesignCorrect() {
            _designModel.Name = TestName;
            _designModel.Selected = Room1;

            _designModel.DesignAdded += (sender, args) => {
                Assert.NotNull(args.Value);
                Assert.AreEqual(TestName, args.Value.Name);
                Assert.AreEqual(Room1, args.Value.Room);
                Assert.IsEmpty(args.Value.ProductPlacements);
            };
            
            _designModel.AddDesign();
        }
    }
}