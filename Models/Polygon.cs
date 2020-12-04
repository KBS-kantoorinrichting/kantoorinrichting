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
        }

        public Polygon(int width, int length) : this(new Position(), width, length) { }

        public Polygon Offset(int xOffset = 0, int yOffset = 0) {
            List<Position> clone = _positions.Select(position => position.CopyAdd(xOffset, yOffset)).ToList();
            return new Polygon(clone);
        }
        
        public string Convert() {
            if (_positions == null) return null;

            if (!_positions.Any()) return "";
            return _positions.Select(p => p.ToString())
                .Aggregate((s1, s2) => $"{s1}|{s2}");
        }

        public IEnumerable<(Position, Position)> GetLines() {
            for (int i = 0; i < _positions.Count; i++) yield return (_positions[i], _positions[(i + 1) % _positions.Count]);
        }
        
        public override string ToString() { return Convert(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        protected bool Equals(Polygon other) {
            return Equals(_positions, other._positions);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Polygon) obj);
        }

        public override int GetHashCode() {
            return (_positions != null ? _positions.GetHashCode() : 0);
        }

        public IEnumerator<Position> GetEnumerator() => _positions.GetEnumerator();
        public Position this[int index] => _positions[index];
        public int Count => _positions.Count;
    }
}