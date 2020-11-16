using System;
using System.Windows.Input;

namespace Designer.ViewModel {
    //Command wat een actie uitvoert geen async
    public class BasicCommand : ICommand {
        private readonly Action _action;
        private bool _disabled;
        public bool Disabled {
            get => _disabled;
            set {
                _disabled = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter) => !Disabled;
        public void Execute(object parameter) => _action?.Invoke();

        public BasicCommand(Action action, bool disabled = false) {
            _action = action;
            Disabled = disabled;
        }

        public event EventHandler CanExecuteChanged;
    }
}