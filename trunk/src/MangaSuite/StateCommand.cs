using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace MangaDownloader
{
    public enum CommandState
    {
        Normal,
        Running,
        Pausing
    }
    public class StateCommand : INotifyPropertyChanged
    {
        private volatile CommandState state;

        public StateCommand(Action<StateCommand> longRunningMethod)
        {
            this.StartCommand = new Captioncommand(() => { if (longRunningMethod != null) { longRunningMethod(this); } },
                () =>
                {
                    return State == CommandState.Normal;
                }, "Start") { };
            this.PauseCommand = new Captioncommand(() => State = CommandState.Pausing, () => { return State == CommandState.Running; }, "Pause");
            this.ResumeCommand = new Captioncommand(
                () => State = CommandState.Running, () => { return State == CommandState.Pausing; }, "Resume");
            this.StopCommand = new Captioncommand(() => State = CommandState.Normal, () => { return State == CommandState.Running || State == CommandState.Pausing; }, "Stop");
            this.State = CommandState.Normal;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Captioncommand PauseCommand { get; set; }

        public Captioncommand ResumeCommand { get; set; }

        public Captioncommand StartCommand { get; set; }

        public CommandState State
        {
            get { return state; }

            set
            {
                state = value;
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    PauseCommand.RaiseCanExecuteChanged();
                    ResumeCommand.RaiseCanExecuteChanged();
                    StartCommand.RaiseCanExecuteChanged();
                    StopCommand.RaiseCanExecuteChanged();
                }));
            }
        }

        public Captioncommand StopCommand { get; set; }

        protected void RaisePropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

}
