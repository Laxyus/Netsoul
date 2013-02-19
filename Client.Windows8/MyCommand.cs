using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Windows8
{
    class MyCommand : ICommand
    {
        Action Command;

        public MyCommand(Action action)
        {
            this.Command = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            this.Command();
        }
    }

    class MyCommand<T> : ICommand
    {
        Action<T> Command;

        public MyCommand(Action<T> action)
        {
            this.Command = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            this.Command((T)parameter);
        }
    }
}
