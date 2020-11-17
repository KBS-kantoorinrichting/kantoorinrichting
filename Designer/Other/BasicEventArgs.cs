namespace Designer.Other {
    public class BasicEventArgs<T> {
        public BasicEventArgs(T value) => Value = value;
        public T Value { get; }
    }
}