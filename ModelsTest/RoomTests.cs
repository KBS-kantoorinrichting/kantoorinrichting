using System.Collections.Generic;
using Models;
using NUnit.Framework;

namespace ModelsTest {
    public class RoomTests {
        public static IEnumerable<TestCaseData> FromListCorrectTestCases {
            get {
                yield return new TestCaseData(new List<Position>(), "");
                yield return new TestCaseData(null, null);
                yield return new TestCaseData(
                    new List<Position> {
                        new Position()
                    }, "0,0"
                );
                yield return new TestCaseData(
                    new List<Position> {
                        new Position(),
                        new Position(),
                        new Position(),
                        new Position()
                    }, "0,0|0,0|0,0|0,0"
                );
                yield return new TestCaseData(
                    new List<Position> {
                        new Position(-100, 100)
                    }, "-100,100"
                );
                yield return new TestCaseData(
                    new List<Position> {
                        new Position(10, -100),
                        new Position(0, 330),
                        new Position(-90),
                        new Position(),
                        new Position(1000)
                    }, "10,-100|0,330|-90,0|0,0|1000,0"
                );
            }
        }

        [Test]
        [TestCase("", 0)]
        [TestCase("1,1", 1)]
        [TestCase("0,1|0,2|2,2|2,1", 4)]
        [TestCase("-1,-10", 1)]
        [TestCase("-1,-10|10,23", 2)]
        public void ToList_Count(string input, int expected) { Assert.AreEqual(expected, Room.ToList(input).Count); }

        [Test]
        [TestCaseSource(nameof(FromListCorrectTestCases))]
        public void ToList_Correct(List<Position> expected, string input) {
            Assert.AreEqual(expected, Room.ToList(input));
        }

        [Test]
        [TestCaseSource(nameof(FromListCorrectTestCases))]
        public void FromList_Correct(List<Position> input, string expected) {
            Assert.AreEqual(expected, Room.FromList(input));
        }

        [Test]
        [TestCase(10, 10, "0,0|10,0|10,10|0,10")]
        [TestCase(100, 10, "0,0|100,0|100,10|0,10")]
        [TestCase(10, 100, "0,0|10,0|10,100|0,100")]
        [TestCase(1, 69, "0,0|1,0|1,69|0,69")]
        public void FromDimensions_Correct(int width, int length, string expected) {
            Assert.AreEqual(expected, Room.FromDimensions(width, length));
        }

        [Test]
        [TestCase("TestCase1", 10, 10, "0,0|10,0|10,10|0,10")]
        [TestCase(": )", 100, 10, "0,0|100,0|100,10|0,10")]
        [TestCase("={", 10, 100, "0,0|10,0|10,100|0,100")]
        [TestCase("1010100030", 1, 69, "0,0|1,0|1,69|0,69")]
        public void FromDimensions_Room_Correct(string name, int width, int length, string expected) {
            Room room = Room.FromDimensions(name, width, length);

            Assert.NotNull(room);
            Assert.AreEqual(name, room.Name);
            Assert.AreEqual(expected, room.Positions);
        }
    }
}