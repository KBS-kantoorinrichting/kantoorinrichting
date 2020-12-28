using System.Collections.Generic;
using Designer.ViewModel;
using Models;
using NUnit.Framework;
using ServicesTest;

namespace DesignerTest {
    public class DesignCatalogTestsStaticMethod : DatabaseTest {
        private static readonly Room Room1 = Room.FromDimensions("TestRoom1", 1, 1);
        private static readonly Room Room2 = Room.FromDimensions("TestRoom2", 2, 4);
        private static readonly Room Room3 = Room.FromDimensions("TestRoom3", 72, 4);

        private static readonly Design Design1 = new Design("TestDesign1", Room1, null);
        private static readonly Design Design2 = new Design("TestDesign2", Room1, null);
        private static readonly Design Design3 = new Design("TestDesign3", Room2, null);
        private static readonly Design Design4 = new Design("TestDesign4", Room3, null);

        protected override List<Room> Rooms => new List<Room> {Room1, Room2};
        protected override List<Design> Designs => new List<Design> {Design1, Design2, Design3, Design4};

        [Test]
        public void LoadDesigns_Count() {
            List<Design> designs = ViewDesigns.LoadDesigns();
            Assert.AreEqual(4, designs.Count);
        }

        [Test]
        public void LoadDesigns_Contains() {
            List<Design> designs = ViewDesigns.LoadDesigns();
            Assert.Contains(Design1, designs);
            Assert.Contains(Design2, designs);
            Assert.Contains(Design3, designs);
            Assert.Contains(Design4, designs);
        }
    }

    public class DesignCatalogTestsInstance : DatabaseTest {
        private static readonly Room Room1 = Room.FromDimensions("TestRoom1", 1, 1);
        private static readonly Room Room2 = Room.FromDimensions("TestRoom2", 2, 4);
        private static readonly Room Room3 = Room.FromDimensions("TestRoom3", 72, 4);

        private static readonly Design Design1 = new Design("TestDesign1", Room1, null);
        private static readonly Design Design2 = new Design("TestDesign2", Room1, null);
        private static readonly Design Design3 = new Design("TestDesign3", Room2, null);
        private static readonly Design Design4 = new Design("TestDesign4", Room3, null);

        private ViewDesigns _designModel;

        protected override List<Room> Rooms => new List<Room> {Room1, Room2};
        protected override List<Design> Designs => new List<Design> {Design1, Design2};

        [SetUp]
        public void Setup() { _designModel = new ViewDesigns(); }

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
        public void DesignSelected_DesignCorrect() {
            _designModel.DesignSelected += (sender, args) => {
                Assert.NotNull(args.Value);
                Assert.AreEqual(Design1.Id, args.Value.Id);
                Assert.AreEqual(Design1.Name, args.Value.Name);
                Assert.AreEqual(Design1.Room, args.Value.Room);
                Assert.AreEqual(Design1.ProductPlacements, args.Value.ProductPlacements);
            };

            _designModel.GotoDesign(Design1);
        }
    }
}