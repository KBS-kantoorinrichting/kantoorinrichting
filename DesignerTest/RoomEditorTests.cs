using System;
using System.Collections.Generic;
using System.Text;
using Designer.Model;
using Designer.View;
using Designer.ViewModel;
using NUnit.Framework;

namespace DesignerTest
{
    class RoomEditorTests
    {
        private string name;
        private int length;
        private int width;

        private string ShouldBeTrue;
        private string ShouldBeFalse;
        //RoomEditorView roomeditorview = new RoomEditorView();
        private RoomEditorViewModel roomeditorviewmodel = new RoomEditorViewModel();


        [SetUp]
        public void Setup()
        {
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
        public void SaveRoom_SameAsReturn()
        {
            // deze methode slaat de kamer op.
            Assert.True(roomeditorviewmodel.SaveRoom(name, width, length));
            Assert.AreEqual(roomeditorviewmodel.room.Name, name);
            Assert.AreEqual(roomeditorviewmodel.room.Length, length);
            Assert.AreEqual(roomeditorviewmodel.room.Width, width);
            
        }


        [Test]
        public void IsTextAllowed()
        {
            // methode controlleerd of de string letters bevat
            Assert.False(roomeditorviewmodel.IsTextAllowed(ShouldBeFalse));
            Assert.True(roomeditorviewmodel.IsTextAllowed(ShouldBeTrue));
        }
  

    }
}
