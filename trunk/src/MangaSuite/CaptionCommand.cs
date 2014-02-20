using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MangaDownloader
{
    public class Captioncommand : ICommand
    {
        private Func<bool> canExecuteMethod;
        private Action executeMethod;

        public Captioncommand(Action executeMethod, Func<bool> canExecuteMethod, string caption)
        {
            this.Caption = caption;
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

        public event EventHandler CanExecuteChanged;

        public string Caption { get; set; }

        public bool CanExecute(object parameter)
        {
            return canExecuteMethod == null ? true : canExecuteMethod();
        }

        public void Execute(object parameter)
        {
            if (executeMethod != null)
                executeMethod();
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
