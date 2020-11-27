using Designer.Utils;
using Models;
using NUnit.Framework;

namespace DesignerTest {
    public class PolyUtilTest {
        [Test]
        public static void MinDistanceTest() {
            PolyUtil.MinDistance(Room.ToList("0,0|10,10|0,10"), Room.ToList("20,0|0,20|20,20"));
        }
    }
}