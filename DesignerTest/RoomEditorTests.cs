using System;
using System.Collections.Generic;
using System.Text;
using Designer.Model;
using Designer.View;
using Designer.ViewModel;
using NUnit.Framework;

namespace DesignerTest {
    class RoomEditorTests {
        private string name;
        private int length;
        private int width;

        private string ShouldBeTrue;

        private string ShouldBeFalse;

        //RoomEditorView roomeditorview = new RoomEditorView();
        private RoomEditorViewModel roomeditorviewmodel = new RoomEditorViewModel();

        [SetUp]
        public void Setup() {
            // maakt neppe database aan
            TestRoomDesignContext.Setup();

            // variabelen voor SaveRoom_SameAsReturn test
            name = "kamernaam_test";
            length = 300;
            width = 300;

            // Variabelen voor de IsTextAllowed test
            ShouldBeTrue = "112233445566778899";
            ShouldBeFalse = ShouldBeTrue + "aabbccdd";
        }

        [Test]
        public void SaveRoom_SameAsReturn() {
            // deze methode slaat de kamer op.

            Room room = RoomEditorViewModel.SaveRoom(name, width, length);
            Assert.NotNull(room);
            Assert.AreEqual(room.Name, name);
            Assert.AreEqual(room.Length, length);
            Assert.AreEqual(room.Width, width);
        }

        // [Test]
        // public void IsTextAllowed() {
        //     // methode controlleerd of de string letters bevat
        //     Assert.False(RoomEditorViewModel.IsNumber(ShouldBeFalse));
        //     Assert.True(RoomEditorViewModel.IsNumber(ShouldBeTrue));
        // }
    }
}