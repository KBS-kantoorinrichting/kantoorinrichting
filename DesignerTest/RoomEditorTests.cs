﻿using Designer.ViewModel;
using Models;
using NUnit.Framework;
using RepositoriesTest;

namespace DesignerTest {
    class RoomEditorTests {
        [SetUp]
        public void Setup() {
            // maakt test database aan
            TestRepository.Setup();
        }

        [Test]
        [TestCase("kamernaam_test", 40, 1000)]
        [TestCase("kamernaam_test1", 230, 1210)]
        [TestCase("kamernaam_test2", 10, 150)]
        [TestCase("kamernaam_test3", 1, 530)]
        [TestCase("kamernaam_test4", 340, 69)]
        public void SaveRoom_SameAsReturn(string name, int width, int length) {
            // deze methode slaat de kamer op.
            Room room = RoomEditorViewModel.SaveRoom(name, width, length);
            
            Assert.NotNull(room);
            Assert.AreEqual(room.Name, name);
            Assert.AreEqual(room.Length, length);
            Assert.AreEqual(room.Width, width);
            Assert.NotNull(room.Id);
        }
    }
}