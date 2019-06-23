using System;
using System.Windows.Input;

namespace IgnoreFiles.Models
{
    public class ActionCommand : ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;
        private bool _canExecuteValue;

        private ActionCommand(Action execute, Func<bool> canExecute, bool initialCanExecute)
        {
            this._canExecuteValue = initialCanExecute;
            this._canExecute = canExecute;
            this._execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public static ICommand Create(Action execute, Func<bool> canExecute = null, bool initialCanExecute = true)
        {
            return new ActionCommand(execute, canExecute, initialCanExecute);
        }

        public static ICommand Create<T>(Action<T> execute, Func<T, bool> canExecute = null, bool initialCanExecute = true)
        {
            return new ActionCommand<T>(execute, canExecute, initialCanExecute);
        }

        public bool CanExecute(object parameter)
        {
            if (this._canExecute == null)
            {
                return true;
            }

            bool oldCanExecute = this._canExecuteValue;
            this._canExecuteValue = this._canExecute();

            if (oldCanExecute ^ this._canExecuteValue)
            {
                this.OnCanExecuteChanged();
            }

            return this._canExecuteValue;
        }

        public void Execute(object parameter)
        {
            this._execute();
        }

        private void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }

    internal class ActionCommand<T> : ICommand
    {
        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _execute;
        private bool _canExecuteValue;

        public ActionCommand(Action<T> execute, Func<T, bool> canExecute, bool initialCanExecute)
        {
            this._execute = execute;
            this._canExecute = canExecute;
            this._canExecuteValue = initialCanExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (this._canExecute == null)
            {
                return true;
            }

            bool oldCanExecute = this._canExecuteValue;
            this._canExecuteValue = this._canExecute((T)parameter);

            if (oldCanExecute ^ this._canExecuteValue)
            {
                this.OnCanExecuteChanged();
            }

            return this._canExecuteValue;
        }

        public void Execute(object parameter)
        {
            this._execute((T)parameter);
        }

        private void OnCanExecuteChanged()
        {
            EventHandler handler = CanExecuteChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
