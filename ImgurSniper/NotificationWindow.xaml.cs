using ImgurSniper.Libraries.Helper;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ImgurSniper {
    /// <summary>
    ///     Interaction logic for NotificationWindow.xaml
    /// </summary>
    public partial class NotificationWindow : IDisposable {
        /// <summary>
        /// Is a Notification currently shown
        /// </summary>
        public static bool IsShown { get; private set; }

        public static readonly Action ActionTroubleshoot =
            delegate { Process.Start(Path.Combine(ConfigHelper.InstallDir, "ImgurSniper.UI.exe"), "Troubleshooting"); };

        public enum NotificationType {
            Progress,
            Success,
            Error
        }
        public const int ShowDuration = 3500;

        private readonly double _left;
        private readonly TaskCompletionSource<bool> _task = new TaskCompletionSource<bool>();
        private bool _autoHide;

        public NotificationWindow(string text, NotificationType type, bool autoHide, Action onClick) {
            InitializeComponent();

            _left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width;
            double top = SystemParameters.WorkArea.Top + SystemParameters.WorkArea.Height;

            Left = _left;
            Top = top - Height - 10;

            _autoHide = autoHide;

            ContentLabel.Text = text;

            switch (type) {
                case NotificationType.Error:
                    ErrorIcon.Visibility = Visibility.Visible;
                    break;
                case NotificationType.Progress:
                    ProgressBar.Visibility = Visibility.Visible;
                    break;
                case NotificationType.Success:
                    SuccessIcon.Visibility = Visibility.Visible;
                    break;
            }

            if (onClick != null) {
                NotificationContent.Cursor = Cursors.Hand;
                NotificationContent.MouseDown += delegate {
                    try {
                        onClick.Invoke();
                    } catch {
                        // ignored
                    }
                    FadeOut();
                };
            }
        }

        ~NotificationWindow() { Dispose(); }

        public NotificationWindow(string text, NotificationType type, bool autoHide) : this(text, type, autoHide, null) { }


        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            IsShown = true;

            FadeIn();

            if (_autoHide) {
                await Task.Delay(ShowDuration);

                FadeOut();
            }
        }

        public Task ShowAsync() {
            _autoHide = true;

            Show();

            return _task.Task;
        }

        public new void Close() {
            FadeOut();
        }

        //Open Animation
        private void FadeIn() {
            this.Animate(OpacityProperty, 0, 1, 200);
            this.Animate(LeftProperty, _left, _left - Width - 10, 150);
        }

        //Close Animation
        private async void FadeOut() {
            try {
                this.Animate(OpacityProperty, 1, 0, 100);
                await this.AnimateAsync(LeftProperty, Left, _left, 100);

                _task.TrySetResult(true);
                GC.Collect();
                base.Close();
            } catch {
                // Invalid Operation, Task Canceled, Wrong Thread...
            } finally {
                IsShown = false;
            }
        }

        private void Window_Close(object sender, MouseButtonEventArgs e) {
            FadeOut();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e) {
            BtnClose.BeginAnimation(OpacityProperty, Animations.FadeIn);
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e) {
            BtnClose.BeginAnimation(OpacityProperty, Animations.FadeOut);
        }

        private void Close_MouseEnter(object sender, MouseEventArgs e) {
            CloseIcon.BeginAnimation(OpacityProperty, Animations.FadeIn);
        }

        private void Close_MouseLeave(object sender, MouseEventArgs e) {
            CloseIcon.BeginAnimation(OpacityProperty, Animations.FadeOut);
        }

        public void Dispose() {
            try {
                Close();
            } catch {
                // ignored
            }
        }
    }
}