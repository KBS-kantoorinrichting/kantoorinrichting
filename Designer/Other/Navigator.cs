using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace Designer.Other {
    public class Navigator : INotifyPropertyChanged {
        private static Navigator _instance;
        public static Navigator Instance {
            get => _instance ??= new Navigator();
            set => _instance = value;
        }

        protected Navigator() {
        }

        private readonly Stack<Page> _stack = new Stack<Page>();

        public Page CurrentPage => _stack.Peek();

        public void Push(Page page) {
            _stack.Push(page);
            OnPropertyChanged();
        }

        public void Replace(Page page) {
            if (CanPop()) _stack.Pop();
            Push(page);
        }

        public void ReplaceAll(Page page) {
            _stack.Clear();
            Push(page);
        }

        public void Pop() {
            if (CanPop()) _stack.Pop();
            OnPropertyChanged();
        }

        public bool CanPop() => _stack.Count != 0;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PageCommand : BasicCommand {
        //Default navigation type is replace all
        public PageCommand(Func<Page> builder = null, NavigationType type = null, bool disabled = false) : base(
            () => {
                Page page = builder?.Invoke();
                type ??= NavigationType.ReplaceAll;
                type.Action?.Invoke(page);
            }, disabled
        ) { }
    }

    public class NavigationType {
        internal readonly Action<Page> Action;

        private NavigationType(Action<Page> action) { Action = action; }

        public static readonly NavigationType Pop = new NavigationType(page => Navigator.Instance.Pop());
        public static readonly NavigationType Replace = new NavigationType(Navigator.Instance.Replace);
        public static readonly NavigationType ReplaceAll = new NavigationType(Navigator.Instance.ReplaceAll);
        public static readonly NavigationType Push = new NavigationType(Navigator.Instance.Push);
    }
}