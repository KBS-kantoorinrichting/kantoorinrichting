using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Models {
    public class Polygon : IReadOnlyList<Position> {
        private readonly List<Position> _positions;

        public Polygon(List<Position> positions) { _positions = positions; }

        public Polygon(string positions) : this(
            positions switch {
                null => new List<Position>(),
                "" => new List<Position>(),
                _ => positions.Split("|").Select(p => new Position(p)).ToList()
            }
        ) {
        }

        public Polygon(Position position, int width, int length) : this(
            new List<Position> {
                position,
                position.CopyAdd(width),
                position.CopyAdd(width, length),
                position.CopyAdd(y: length)
            }
        ) {
            _min = position;
            _max = position.CopyAdd(width, length);
            _center = position.CopyAdd(width / 2, length / 2);
            _bounds = this;
        }

        public Polygon(int width, int length) : this(new Position(), width, length) { }

        public Polygon Offset(int xOffset = 0, int yOffset = 0) {
            List<Position> clone = _positions.Select(position => position.CopyAdd(xOffset, yOffset)).ToList();
            Polygon poly = new Polygon(clone);
            if (_min != null) poly._min = _min.CopyAdd(xOffset, yOffset);
            if (_max != null) poly._max = _max.CopyAdd(xOffset, yOffset);
            if (_center != null) poly._center = _center.CopyAdd(xOffset, yOffset);
            if (Equals(_bounds)) poly._bounds = poly;
            else if (_bounds != null) poly._bounds = _bounds.Offset(xOffset, yOffset);
            return poly;
        }

        public Polygon Offset(Position position) { return Offset(position.X, position.Y); }

        /*
         * Geeft lijst van posities terug 
         */
        public List<Position> GetPositions()
        {
            return _positions;
        }
        
        /**
         * Zet de polygon om naar een tekst van die lijkt op 0,0|10,10|20,10
         */
        public string Convert(string separator = "|") {
            if (_positions == null) return null;

            if (!_positions.Any()) return "";
            return _positions.Select(p => p.ToString())
                .Aggregate((s1, s2) => $"{s1}{separator}{s2}");
        }

        /**
         * Maakt alle opvolgende combinatie van lijnen
         */
        public IEnumerable<Line> GetLines() {
            for (int i = 0; i < _positions.Count; i++) {
                Position p1 = _positions[i];
                Position p2 = _positions[(i + 1) % _positions.Count];
                if (!Equals(p1, p2)) yield return new Line(_positions[i], _positions[(i + 1) % _positions.Count]);
            }
        }

        public override string ToString() { return Convert(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        protected bool Equals(Polygon other) {
            return Equals(_positions, other?._positions);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Polygon) obj);
        }

        public override int GetHashCode() { return (_positions != null ? _positions.GetHashCode() : 0); }

        private Position _max;
        private Position _min;
        private Position _center;
        private Polygon _bounds;

        public Position Max() {
            return _max ??= _positions.Aggregate((p1, p2) => new Position(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y)));
        }

        public Position Min() {
            return _min ?? _positions.Aggregate((p1, p2) => new Position(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y)));
        }

        public int Width => Max().X - Min().X;
        public int Length => Max().Y - Min().Y;

        public Position Center() {
            if (_center != null) return _center;

            Position min = Min();
            Position max = Max();
            return _center = new Position((min.X + max.X) / 2, ((min.Y + max.Y) / 2));
        }

        public Polygon Bounds() {
            if (_bounds != null) return _bounds;

            Position min = Min();
            Position max = Max();
            return _bounds = new Polygon(
                new List<Position> {min, new Position(min.X, max.Y), max, new Position(max.X, min.Y)}
            );
        }

        public IEnumerator<Position> GetEnumerator() => _positions.GetEnumerator();
        public Position this[int index] => _positions[index];
        public int Count => _positions.Count;
    }

    public class Line {
        public Position P1 { get; }
        public Position P2 { get; }

        public Line(Position p1, Position p2) {
            P1 = p1;
            P2 = p2;
        }

        public Line Offset(int x = 0, int y = 0) => new Line(P1.CopyAdd(x, y), P2.CopyAdd(x, y));
        
        public (Position p1, Position p2) AsTuple => (P1, P2);

        public Position Center => new Position((P1.X + P2.X) / 2, (P1.Y + P2.Y) / 2);
        
        public List<Position> AsList() => new List<Position> {P1, P2};
    }
}