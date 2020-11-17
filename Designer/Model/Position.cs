namespace Designer.Model {
    public class Position {
        public int X { get; }
        public int Y { get; }

        public Position(int x, int y) {
            X = x;
            Y = y;
        }

        public Position(string s) {
            string[] parts = s.Split(",");
            X = int.Parse(parts[0]);
            Y = int.Parse(parts[1]);
        }

        public override string ToString() => $"${X},${Y}";
    }
}