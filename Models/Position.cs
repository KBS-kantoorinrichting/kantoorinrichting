using System;

namespace Models {
    public class Position {
        public int X { get; }
        public int Y { get; }

        public Position(int x = 0, int y = 0) {
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

        public Position Center(Position position) {
            return new Position((X + position.X) / 2, (Y + position.Y) / 2);
        }

        public double Distance(Position p) {
            return Distance(p.X, p.Y);
        }

        public double Distance(double x, double y) {
            return Math.Sqrt(Math.Pow(Y - y, 2) + Math.Pow(X - x, 2));
        }

        public double DistanceManhattan(Position p) {
            return DistanceManhattan(p.X, p.Y);
        }

        public double DistanceManhattan(double x, double y) {
            return Math.Abs(Y - y) + Math.Abs(X - x);
        }

        public Position Copy() {
            return new Position(X, Y);
        }

        public Position CopyWith(int? x = null, int? y = null) {
            return new Position(x ?? X, y ?? Y);
        }

        public Position CopyAdd(int x = 0, int y = 0) {
            return new Position(X + x, Y + y);
        }
    }
}