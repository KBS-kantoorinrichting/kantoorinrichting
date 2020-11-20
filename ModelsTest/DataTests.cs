using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using NUnit.Framework;

namespace ModelsTest {
    public abstract class DataTests {
        public void Data_Equals_All(Data d1, Data d2) {
            Data_Equals(d1, d2, true);
            Data_Hashcode_Equals(d1, d2);
            Data_Equals_Equals(d1, d2, true);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public void Data_Equals(Data d1, Data d2, bool equals) {
            if (equals) Assert.AreEqual(d1, d2);
            else Assert.AreNotEqual(d1, d2);
        }
        
        public void Data_Hashcode_Equals(Data d1, Data d2) {
            Assert.AreEqual(d1, d2);
        }

        [Test]
        [TestCaseSource("TestCases")]
        public void Data_Equals_Equals(Data d1, Data d2, bool equals) {
            Assert.AreEqual(equals, d1?.Equals(d2) ?? d2 == null);
            Assert.AreEqual(equals, d2?.Equals(d1) ?? d1 == null);
        }
    }

    public class DataProductsTests : DataTests {
        private static IEnumerable<TestCaseData> TestCases {
            get {
                yield return new TestCaseData(new Product(), null, false);
                yield return new TestCaseData(new Product(""), new Product(), false);
                yield return new TestCaseData(new Product("klaas"), new Product(""), false);
                yield return new TestCaseData(new Product("12121212"), new Product("3131231232131"), false);
                yield return new TestCaseData(new Product(" "), new Product("  "), false);
                yield return new TestCaseData(new Product("\r"), new Product("\n"), false);
                yield return new TestCaseData(new Product(id: -1), new Product(id: 1), false);
                yield return new TestCaseData(new Product(id: 100), new Product(id: 1), false);
                yield return new TestCaseData(new Product(id: -1337), new Product(id: 1337), false);
                yield return new TestCaseData(new Product(id: int.MaxValue), new Product(id: int.MinValue), false);
                yield return new TestCaseData(new Product(width: -1), new Product(width: 1), false);
                yield return new TestCaseData(new Product(width: 100), new Product(width: 1), false);
                yield return new TestCaseData(new Product(width: -1337), new Product(width: 1337), false);
                yield return new TestCaseData(new Product(width: int.MaxValue), new Product(width: int.MinValue), false);
                yield return new TestCaseData(new Product(length: -1), new Product(length: 1), false);
                yield return new TestCaseData(new Product(length: 100), new Product(length: 1), false);
                yield return new TestCaseData(new Product(length: -1337), new Product(length: 1337), false);
                yield return new TestCaseData(new Product(length: int.MaxValue), new Product(length: int.MinValue), false);
                yield return new TestCaseData(new Product(price: -1), new Product(price: 1), false);
                yield return new TestCaseData(new Product(price: 100), new Product(price: 1), false);
                yield return new TestCaseData(new Product(price: -1337), new Product(price: 1337), false);
                yield return new TestCaseData(new Product(price: double.Epsilon), new Product(price: 0), false);
                yield return new TestCaseData(new Product(price: double.NegativeInfinity), new Product(price: double.PositiveInfinity), false);
                yield return new TestCaseData(new Product(price: double.NaN), new Product(price: -1), false);
                yield return new TestCaseData(new Product(price: double.MaxValue), new Product(price: double.MinValue), false);
                yield return new TestCaseData(new Product(photo: ""), new Product(), false);
                yield return new TestCaseData(new Product(photo: "klaas"), new Product(photo: ""), false);
                yield return new TestCaseData(new Product(photo: "12121212"), new Product(photo: "3131231232131"), false);
                yield return new TestCaseData(new Product(photo: " "), new Product(photo: "  "), false);
                yield return new TestCaseData(new Product(photo: "\r"), new Product(photo: "\n"), false);
            }
        }

        [Test]
        public void Product_Mass_Test() {
            List<int> possibleIds = new List<int> {0, 1, 10, 13, 1337, -1, -10, -13, -1337};
            List<string> possibleNames = new List<string> {null, "", "10", "Klaas", "Jan", "--12-&@**&#&*@#"};
            List<int> possibleWidths = possibleIds;
            List<int> possibleLengths = possibleIds;
            List<double?> possiblePrices = new List<double?>
                {null, 0, 1, 1337, 10.1, 13.34, 1337.19003, -1, -1337, -10.1, -13.34, -1337.19003, Math.PI, -Math.PI};
            List<string> possiblePhotos = new List<string>
                {null, "", "test.png", "ander.jpeg", "geen file", "rare tekens $%&*$&^"};

            long amount = 1;
            Data_Equals_All(null, null);
            foreach ((Product p1, Product p2) in from id in possibleIds
                from name in possibleNames
                from width in possibleWidths
                from length in possibleLengths
                from price in possiblePrices
                from photo in possiblePhotos
                let p1 = new Product(name, id, width, length, price, photo)
                let p2 = new Product(name, id, width, length, price, photo)
                select (p1, p2)) {
                Data_Equals_All(p1, p2);
                amount++;
            }

            Console.WriteLine($"Test cases tested: {amount}");
        }
    }

    public class DataRoomTests : DataTests {
        private static IEnumerable<TestCaseData> TestCases {
            get {
                yield return new TestCaseData(new Room(), null, false);
                yield return new TestCaseData(new Room(""), new Room(), false);
                yield return new TestCaseData(new Room("klaas"), new Room(""), false);
                yield return new TestCaseData(new Room("12121212"), new Room("3131231232131"), false);
                yield return new TestCaseData(new Room(" "), new Room("  "), false);
                yield return new TestCaseData(new Room("\r"), new Room("\n"), false);
                yield return new TestCaseData(new Room(id: -1), new Room(id: 1), false);
                yield return new TestCaseData(new Room(id: 100), new Room(id: 1), false);
                yield return new TestCaseData(new Room(id: -1337), new Room(id: 1337), false);
                yield return new TestCaseData(new Room(id: int.MaxValue), new Room(id: int.MinValue), false);
                yield return new TestCaseData(new Room(width: -1), new Room(width: 1), false);
                yield return new TestCaseData(new Room(width: 100), new Room(width: 1), false);
                yield return new TestCaseData(new Room(width: -1337), new Room(width: 1337), false);
                yield return new TestCaseData(new Room(width: int.MaxValue), new Room(width: int.MinValue), false);
                yield return new TestCaseData(new Room(length: -1), new Room(length: 1), false);
                yield return new TestCaseData(new Room(length: 100), new Room(length: 1), false);
                yield return new TestCaseData(new Room(length: -1337), new Room(length: 1337), false);
                yield return new TestCaseData(new Room(length: int.MaxValue), new Room(length: int.MinValue), false);
            }
        }

        [Test]
        public void Room_Mass_Test() {
            List<int> possibleIds = new List<int> {0, 1, 10, 13, 1337, -1, -10, -13, -1337};
            List<string> possibleNames = new List<string> {null, "", "10", "Klaas", "Jan", "--12-&@**&#&*@#"};
            List<int> possibleWidths = possibleIds;
            List<int> possibleLengths = possibleIds;

            long amount = 1;
            Data_Equals_All(null, null);
            foreach ((Room r1, Room r2) in from id in possibleIds
                from name in possibleNames
                from width in possibleWidths
                from length in possibleLengths
                let r1 = new Room(name, width, length, id)
                let r2 = new Room(name, width, length, id)
                select (r1, r2)) {
                Data_Equals_All(r1, r2);
                amount++;
            }

            Console.WriteLine($"Test cases tested: {amount}");
        }
    }
}