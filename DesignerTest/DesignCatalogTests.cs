using System.Collections.Generic;
using Designer.Model;
using Designer.Other;
using Designer.ViewModel;
using NUnit.Framework;

namespace DesignerTest {
    public class DesignCatalogTestsStaticMethod {
        private static readonly Room Room1 = new Room("TestRoom1", 1, 1);
        private static readonly Room Room2 = new Room("TestRoom2", 2, 4);
        private static readonly Room Room3 = new Room("TestRoom3", 72, 4);
        
        private static readonly Design Design1 = new Design("TestDesign1", Room1, null);
        private static readonly Design Design2 = new Design("TestDesign2", Room1, null);
        private static readonly Design Design3 = new Design("TestDesign3", Room2, null);
        private static readonly Design Design4 = new Design("TestDesign4", Room3, null);

        [SetUp]
        public void Setup() {
            TestRoomDesignContext.Setup(
                new List<Room> {
                    Room1, Room2
                },
                new List<Design> {
                    Design1, Design2, Design3, Design4
                }
            );
        }

        [Test]
        public void LoadDesigns_Count() {
            List<Design> designs = DesignCatalogModel.LoadDesigns();
            Assert.AreEqual(4, designs.Count);
        }

        [Test]
        public void LoadDesigns_Contains() {
            List<Design> designs = DesignCatalogModel.LoadDesigns();
            Assert.Contains(Design1, designs);
            Assert.Contains(Design2, designs);
            Assert.Contains(Design3, designs);
            Assert.Contains(Design4, designs);
        }
    }
    
    public class DesignCatalogTestsInstance {
        private static readonly Room Room1 = new Room("TestRoom1", 1, 1);
        private static readonly Room Room2 = new Room("TestRoom2", 2, 4);
        private static readonly Room Room3 = new Room("TestRoom3", 72, 4);
        
        private static readonly Design Design1 = new Design("TestDesign1", Room1, null);
        private static readonly Design Design2 = new Design("TestDesign2", Room1, null);
        private static readonly Design Design3 = new Design("TestDesign3", Room2, null);
        private static readonly Design Design4 = new Design("TestDesign4", Room3, null);
        
        private DesignCatalogModel _designModel;
        
        [SetUp]
        public void Setup() {
            TestRoomDesignContext.Setup(
                new List<Room> {
                    Room1, Room2
                },
                new List<Design> {
                    Design1, Design2
                }
            );
            _designModel = new DesignCatalogModel();
        }

        [Test]
        public void Loads_Count() {
            Assert.AreEqual(2, _designModel.Designs.Count);
        }

        [Test]
        public void Loads_Contains() {
            List<Design> designs = _designModel.Designs;
            Assert.Contains(Design1, designs);
            Assert.Contains(Design2, designs);
        }

        [Test]
        public void NeedsReload_Count() {
            RoomDesignContext.Instance.Designs.Add(Design3);
            
            List<Design> designs = _designModel.Designs;
            Assert.AreEqual(2, designs.Count);
        }

        [Test]
        public void NeedsReload_Contains() {
            RoomDesignContext.Instance.Designs.Add(Design3);
            
            List<Design> designs = _designModel.Designs;
            Assert.Contains(Design1, designs);
            Assert.Contains(Design2, designs);
            Assert.IsFalse(designs.Contains(Design3));
        }

        [Test]
        public void Reload_Count() {
            RoomDesignContext.Instance.Designs.Add(Design3);
            RoomDesignContext.Instance.SaveChanges();
            _designModel.Reload();
            
            List<Design> designs = _designModel.Designs;
            Assert.AreEqual(3, designs.Count);
        }

        [Test]
        public void Reload_Contains() {
            RoomDesignContext.Instance.Designs.Add(Design3);
            RoomDesignContext.Instance.SaveChanges();
            _designModel.Reload();
            
            List<Design> designs = _designModel.Designs;
            Assert.Contains(Design1, designs);
            Assert.Contains(Design2, designs);
            Assert.Contains(Design3, designs);
        }

        [Test]
        public void ReloadCommand_Count() {
            RoomDesignContext.Instance.Designs.Add(Design3);
            RoomDesignContext.Instance.SaveChanges();
            _designModel.ReloadCommand.Execute(null);
            
            List<Design> designs = _designModel.Designs;
            Assert.AreEqual(3, designs.Count);
        }

        [Test]
        public void ReloadCommand_Contains() {
            RoomDesignContext.Instance.Designs.Add(Design3);
            RoomDesignContext.Instance.SaveChanges();
            _designModel.ReloadCommand.Execute(null);
            
            List<Design> designs = _designModel.Designs;
            Assert.Contains(Design1, designs);
            Assert.Contains(Design2, designs);
            Assert.Contains(Design3, designs);
        }

        [Test]
        public void Selected_TriggersEvent() {
            bool triggered = false;

            _designModel.DesignSelected += (sender, args) => triggered = true;
            _designModel.Selected = Design1;

            Assert.IsTrue(triggered);
        }

        [Test]
        public void GotoDesign_TriggersEvent() {
            bool triggered = false;

            _designModel.DesignSelected += (sender, args) => triggered = true;
            _designModel.GotoDesign(Design1);

            Assert.IsTrue(triggered);
        }

        [Test]
        public void PageOnDesignAdded_TriggersEvent() {
            bool triggered = false;

            _designModel.DesignSelected += (sender, args) => triggered = true;
            _designModel.PageOnDesignAdded(this, new BasicEventArgs<Design>(Design1));

            Assert.IsTrue(triggered);
        }

        [Test]
        public void DesignSelected_DesignCorrect() {
            _designModel.DesignSelected += (sender, args) => {
                Assert.NotNull(args.Value);
                Assert.AreEqual(Design1.DesignId, args.Value.DesignId);
                Assert.AreEqual(Design1.Name, args.Value.Name);
                Assert.AreEqual(Design1.Room, args.Value.Room);
                Assert.AreEqual(Design1.ProductPlacements, args.Value.ProductPlacements);
            };
            
            _designModel.GotoDesign(Design1);
        }
    }

    public class DesignCatalogTestsAddDesignIntegration {
        private static readonly Room Room1 = new Room("TestRoom1", 1, 1);
        private static readonly Room Room2 = new Room("TestRoom2", 2, 4);
        
        private static readonly Design Design1 = new Design("TestDesign1", Room1, null);
        private static readonly Design Design2 = new Design("TestDesign2", Room1, null);
        
        private static readonly string TestName = "TestDesign";
        
        private AddDesignModel _addDesignModel;
        private DesignCatalogModel _designCatalogModel;
        
        [SetUp]
        public void Setup() {
            TestRoomDesignContext.Setup(
                new List<Room> {
                    Room1, Room2
                },
                new List<Design> {
                    Design1, Design2
                }
            );
            _addDesignModel = new AddDesignModel();
            _designCatalogModel = new DesignCatalogModel();

            _addDesignModel.DesignAdded += _designCatalogModel.PageOnDesignAdded;
        }
        
        [Test]
        public void DesignAdded_TriggersSelectedEvent() {
            _addDesignModel.Name = TestName;
            _addDesignModel.Selected = Room1;

            bool triggered = false;

            _designCatalogModel.DesignSelected += (sender, args) => triggered = true;
            _addDesignModel.AddDesign();

            Assert.IsTrue(triggered);
        }
        
        [Test]
        public void DesignAdded_DesignCorrect() {
            _addDesignModel.Name = TestName;
            _addDesignModel.Selected = Room1;

            _designCatalogModel.DesignSelected += (sender, args) => {
                Assert.NotNull(args.Value);
                Assert.AreEqual(TestName, args.Value.Name);
                Assert.AreEqual(Room1, args.Value.Room);
                Assert.IsEmpty(args.Value.ProductPlacements);
            };
            
            _addDesignModel.AddDesign();
        }
        
        [Test]
        public void DesignAdded_DesignsUpdates() {
            _addDesignModel.Name = TestName;
            _addDesignModel.Selected = Room1;

            _designCatalogModel.DesignSelected += (sender, args) => {
                Assert.Contains(args.Value, _designCatalogModel.Designs);
            };
            
            _addDesignModel.AddDesign();
        }
    }
}