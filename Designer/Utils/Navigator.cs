﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace Designer.Utils {
    public class Navigator : INotifyPropertyChanged {
        //Maakt een "singleton" omdat dit voor nu een makelijk manier is om in elke viewmodel er bij te kunnen.
        private static Navigator _instance;

        //De stack houdt bij welke pagina open staan
        private readonly Stack<Page> _stack = new Stack<Page>();

        private Navigator() { }

        public static Navigator Instance {
            get => _instance ??= new Navigator();
            set => _instance = value;
        }

        /**
         * De momentelen pagina laat altijd de laatste zien
         */
        public Page CurrentPage => _stack.Count > 0 ? _stack.Peek() : null;

        public event PropertyChangedEventHandler PropertyChanged;

        /**
         * Laat je pagina zien zonder de oude te verwijderen
         */
        public void Push(Page page) {
            _stack.Push(page);
            OnPropertyChanged();
        }

        /**
         * Laat je pagina zien en vervang de laaste pagina
         */
        public void Replace(Page page) {
            if (CanPop()) _stack.Pop();
            Push(page);
        }

        /**
         * Laat je pagina zien en vervang alle paginas
         */
        public void ReplaceAll(Page page) {
            _stack.Clear();
            Push(page);
        }

        /**
         * Gaat terug naar de vorige pagina
         */
        public void Pop() {
            if (CanPop()) _stack.Pop();
            OnPropertyChanged();
        }

        /**
         * True als er een pagina wordt weergegeven anders False
         */
        public bool CanPop() { return _stack.Count != 0; }

        private void OnPropertyChanged(string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    //Een command die de verschillende navigator methodes kan uitvoeren
    public class PageCommand : BasicCommand {
        /**
         * Standaart NavigationType is ReplaceAll
         * Builder is verplicht als het type niet Pop is
         */
        public PageCommand(Func<Page> builder = null, NavigationType type = null, bool disabled = false) : base(
            () => {
                Page page = builder?.Invoke();
                type ??= NavigationType.ReplaceAll;
                type.Action?.Invoke(page);
            }, disabled
        ) {
        }
    }

    public class NavigationType {
        public static readonly NavigationType Pop = new NavigationType(page => Navigator.Instance.Pop());
        public static readonly NavigationType Replace = new NavigationType(Navigator.Instance.Replace);
        public static readonly NavigationType ReplaceAll = new NavigationType(Navigator.Instance.ReplaceAll);
        public static readonly NavigationType Push = new NavigationType(Navigator.Instance.Push);
        internal readonly Action<Page> Action;

        private NavigationType(Action<Page> action) { Action = action; }
    }
}