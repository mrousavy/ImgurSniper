using System;
using System.Diagnostics;
using System.Windows.Input;

namespace ImgurSniper {
    public class Commands {
        public class LoginCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) {
                return true;
            }

            public void Execute(object parameter) {
                //TODO: Login to Imgur
            }
        }

        public class RetryCommand : ICommand {
            public event EventHandler CanExecuteChanged;

            public bool CanExecute(object parameter) {
                return true;
            }

            public void Execute(object parameter) {
                Process process = Process.GetCurrentProcess();
                process.Start();
            }
        }
    }
}
