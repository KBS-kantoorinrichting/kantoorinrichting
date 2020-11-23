using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Designer.Other {
    //Command wat een actie uitvoert geen async
    public class BasicCommand : ICommand {
        private readonly Action _action;
        private bool _disabled;

        public BasicCommand(Action action, bool disabled = false) {
            _action = action;
            Disabled = disabled;
        }

        public bool Disabled {
            get => _disabled;
            set {
                _disabled = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter) { return !Disabled; }

        public virtual void Execute(object parameter) {
            _action?.Invoke();
        }

        public event EventHandler CanExecuteChanged;
    }

    //Command wat een actie uitvoert met een argument
    public class ArgumentCommand<T> : BasicCommand {
        private readonly Action<T> _action;

        public ArgumentCommand(Action<T> action, bool disabled = false) : base(null, disabled) { _action = action; }

        public override void Execute(object parameter) {
            if (parameter is T o) _action?.Invoke(o);
            else Debug.WriteLine("Recieved wrong parameter for this command" + parameter?.GetType());
        }
    }
}