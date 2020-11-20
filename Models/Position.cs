using System;

namespace Models {
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

        public override string ToString() => $"{X},{Y}";

        protected bool Equals(Position other) {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Position) obj);
        }

        public override int GetHashCode() {
            return HashCode.Combine(X, Y);
        }
    }
}