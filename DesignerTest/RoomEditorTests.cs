using Designer.ViewModel;
using Models;
using NUnit.Framework;
using ServicesTest;

namespace DesignerTest {
    class RoomEditorTests : DatabaseTest {

        [Test]
        [TestCase("kamernaam_test", 40, 1000)]
        [TestCase("kamernaam_test1", 230, 1210)]
        [TestCase("kamernaam_test2", 10, 150)]
        [TestCase("kamernaam_test3", 1, 530)]
        [TestCase("kamernaam_test4", 340, 69)]
        public void SaveRoomWithoutTemplate_SameAsReturn(string name, int width, int length) {
            // deze methode slaat de kamer op.
            Room room = RoomTemplateViewModel.SaveRoom(name, width, length);
            
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
            Room room = RoomTemplateViewModel.SaveRoom(name, width, length, template);

            Assert.NotNull(room);
            Assert.AreEqual(room.Name, name);
            Assert.AreEqual(room.Positions, Room.FromTemplate(width, length, template));
            Assert.NotNull(room.Id);
        }
    }
}