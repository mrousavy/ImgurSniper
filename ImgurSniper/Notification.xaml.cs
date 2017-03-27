using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ImgurSniper {
    /// <summary>
    ///     Interaction logic for Notification.xaml
    /// </summary>
    public partial class Notification : Window {
        public enum NotificationType {
            Progress,
            Success,
            Error
        }

        private readonly double _left;
        private readonly TaskCompletionSource<bool> _task = new TaskCompletionSource<bool>();
        private bool _autoHide;

        public Notification(string text, NotificationType type, bool autoHide, Action onClick) {
            InitializeComponent();

            _left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width;
            double top = SystemParameters.WorkArea.Top + SystemParameters.WorkArea.Height;

            Left = _left;
            Top = top - Height - 10;

            _autoHide = autoHide;

            contentLabel.Text = text;

            switch (type) {
                case NotificationType.Error:
                    errorIcon.Visibility = Visibility.Visible;
                    break;
                case NotificationType.Progress:
                    progressBar.Visibility = Visibility.Visible;
                    break;
                case NotificationType.Success:
                    successIcon.Visibility = Visibility.Visible;
                    break;
            }

            if (onClick != null) {
                NotificationContent.Cursor = Cursors.Hand;
                NotificationContent.MouseDown += delegate {
                    try {
                        onClick.Invoke();
                    }
                    catch {}
                    FadeOut();
                };
            }
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            FadeIn();

            if (_autoHide) {
                await Task.Delay(3000);

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
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            DoubleAnimation slideInX = new DoubleAnimation(_left, _left - Width - 10, TimeSpan.FromMilliseconds(150));

            BeginAnimation(LeftProperty, slideInX);
            BeginAnimation(OpacityProperty, fadeIn);
        }

        //Close Animation
        private void FadeOut() {
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(100));
            DoubleAnimation slideOutX = new DoubleAnimation(Left, _left, TimeSpan.FromMilliseconds(80));

            fadeOut.Completed += delegate {
                try {
                    _task.SetResult(true);
                }
                catch {}
                base.Close();
            };

            BeginAnimation(LeftProperty, slideOutX);
            BeginAnimation(OpacityProperty, fadeOut);
        }

        private void Window_Close(object sender, MouseButtonEventArgs e) {
            FadeOut();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e) {
            Btn_Close.BeginAnimation(OpacityProperty, Animations.FadeIn);
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e) {
            Btn_Close.BeginAnimation(OpacityProperty, Animations.FadeOut);
        }

        private void Close_MouseEnter(object sender, MouseEventArgs e) {
            CloseIcon.BeginAnimation(OpacityProperty, Animations.FadeIn);
        }

        private void Close_MouseLeave(object sender, MouseEventArgs e) {
            CloseIcon.BeginAnimation(OpacityProperty, Animations.FadeOut);
        }
    }
}