using Designer.ViewModel;
using Models;
using NUnit.Framework;
using ServicesTest;

namespace DesignerTest {
    public class MakeRoomTest : DatabaseTest {
        private static readonly RoomEditor ViewModel = new RoomEditor("test");

        [SetUp]
        public void Init()
        {
            ViewModel.SelectedPoints.Add(new Position(0, 0));
            ViewModel.SelectedPoints.Add(new Position(0, 150));
            ViewModel.SelectedPoints.Add(new Position(150, 0));
            ViewModel.SelectedPoints.Add(new Position(150, 150));
        }

        [Test]
        [TestCase("kamernaam_test", 40, 1000)]
        [TestCase("kamernaam_test1", 230, 1210)]
        [TestCase("kamernaam_test2", 10, 150)]
        [TestCase("kamernaam_test3", 1, 530)]
        [TestCase("kamernaam_test4", 340, 69)]
        public void SaveRoomWithoutTemplate_SameAsReturn(string name, int width, int length) {
            // deze methode slaat de kamer op.
            Room room = RoomTemplate.SaveRoom(name, width, length);
            
            Assert.NotNull(room);
            Assert.AreEqual(room.Name, name);
            Assert.AreEqual(room.Positions, Room.FromDimensions(width, length));
            Assert.NotNull(room.Id);
        }

        [Test]
        [TestCase("kamernaam_test5", 40, 1000, 0)]
        [TestCase("kamernaam_test6", 230, 1210, 0)]
        [TestCase("kamernaam_test7", 10, 150, 0)]
        [TestCase("kamernaam_test8", 1, 530, 0)]
        [TestCase("kamernaam_test9", 340, 69, 0)]
        [TestCase("kamernaam_test10", 40, 1000, 1)]
        [TestCase("kamernaam_test11", 230, 1210, 1)]
        [TestCase("kamernaam_test12", 10, 150, 1)]
        [TestCase("kamernaam_test13", 1, 530, 1)]
        [TestCase("kamernaam_test14", 340, 69, 1)]
        public void SaveRoomWithTemplate_SameAsReturn(string name, int width, int length, int template)
        {
            // deze methode slaat de kamer op.
            Room room = RoomTemplate.SaveRoom(name, width, length, template);

            Assert.NotNull(room);
            Assert.AreEqual(room.Name, name);
            Assert.AreEqual(room.Positions, Room.FromTemplate(width, length, template));
            Assert.NotNull(room.Id);
        }

      /*  [Test]
        public void RoomFromList_SameAsReturn(IEnumerable<Position> positions)
        {
            string room = Room.FromList(positions);
            niet mogelijk?
        }*/

        [Test]
        [TestCase("kamernaam_test5", 40, 1000)]
        [TestCase("kamernaam_test6", 230, 1210)]
        [TestCase("kamernaam_test7", 10, 150)]
        [TestCase("kamernaam_test8", 1, 530)]
        [TestCase("kamernaam_test9", 340, 69)]
        [TestCase("kamernaam_test10", 40, 1000)]
        [TestCase("kamernaam_test11", 230, 1210)]
        [TestCase("kamernaam_test12", 10, 150)]
        [TestCase("kamernaam_test13", 1, 530)]
        [TestCase("kamernaam_test14", 340, 69)]
        public void RoomFromDimensions_SameAsReturn(string name, int width, int length)
        {
            Room room = Room.FromDimensions(name, width, length);

            Assert.NotNull(room);
            Assert.AreEqual(room.Name, name);
            Assert.AreEqual(room.Positions, Room.FromDimensions(width,length));

        }

        // ui error
        [Test]
        [TestCase("kamernaam_test5", 40, 1000)]
        [TestCase("kamernaam_test6", 230, 1210)]
        [TestCase("kamernaam_test7", 10, 150)]
        [TestCase("kamernaam_test8", 1, 530)]
        [TestCase("kamernaam_test9", 340, 69)]
        [TestCase("kamernaam_test10", 40, 1000)]
        [TestCase("kamernaam_test11", 230, 1210)]
        [TestCase("kamernaam_test12", 10, 150)]
        [TestCase("kamernaam_test13", 1, 530)]
        [TestCase("kamernaam_test14", 340, 69)]
        public void MakeRoom_Test(string name, int width, int length)
        {
            //TODO
            Assert.Inconclusive("Deze test werkt niet!");
            Room room = Room.FromDimensions(name, width, length);
            RoomEditor revm = new RoomEditor();

            Assert.NotNull(revm.MakeRoom(room));
            Assert.AreEqual(revm.MakeRoom(room), room.Positions);

        }

        [Test]
        [TestCase(50, 150, ExpectedResult = true)]
        [TestCase(24, 150, ExpectedResult = true)]
        [TestCase(46, 150, ExpectedResult = true)]
        [TestCase(50, 50, ExpectedResult = false)]
        [TestCase(150, 0, ExpectedResult = false)]
        public bool RoomEditorViewModel_WithinSelectedPoints_ShouldReturnBoolean(int x, int y)
        {
            return ViewModel.WithinSelectedPoints(x, y);
        }

        [Test]
        public void RoomEditorViewModel_CalculateNextPositionExample_ShouldReturnPosition()
        {
            Position nextPosition = ViewModel.CalculateNextPositionFromAngle(180, 25, 50);

            Assert.IsNotNull(nextPosition);
            Assert.AreEqual(nextPosition, new Position(25, 75));
        }
    }
}