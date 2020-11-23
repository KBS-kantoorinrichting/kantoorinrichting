using System.Runtime.CompilerServices;

namespace Models {
    public interface IEntity {
        /**
         * De primary key van de entity
         */
        public int Id { get; set; }
    }

    public abstract class Data {
        protected abstract ITuple Variables { get; }

        public override bool Equals(object? obj) {
            return GetType() == obj?.GetType() && Variables.Equals((obj as Data)?.Variables);
        }

        public override int GetHashCode() => Variables.GetHashCode();

        public override string ToString() => Variables.ToString();
    }
}