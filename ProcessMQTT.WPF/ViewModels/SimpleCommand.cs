using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProcessMQTT.WPF.ViewModels
{
    public class SimpleCommand : ICommand
    {
        private Action action;
        public SimpleCommand(Action _action)
        {
            this.action = _action;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            action();
        }
    }
}
