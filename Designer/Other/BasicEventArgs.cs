namespace Designer.Other {
    public class BasicEventArgs<T> {
        public T Value { get; }
        
        public BasicEventArgs(T value) { Value = value; }
    }
}