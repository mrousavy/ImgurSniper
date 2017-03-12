using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for Notification.xaml
    /// </summary>
    public partial class Notification : Window {
        private double _top;
        private double _left;
        private bool _autoHide;

        public Notification(string text, bool showLoading, bool autoHide) {
            InitializeComponent();

            _left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width;
            _top = SystemParameters.WorkArea.Top + SystemParameters.WorkArea.Height;

            Left = _left;
            Top = _top - Height - 10;

            _autoHide = autoHide;

            contentLabel.Content = text;
            progressBar.Visibility = showLoading ? Visibility.Visible : Visibility.Collapsed;
        }

        public Notification(string text, bool showLoading, bool autoHide,
            string btn1Text, string btn2Text, RoutedEventHandler btn1Action, RoutedEventHandler btn2Action) : this(text, showLoading, autoHide) {
            Buttons.Visibility = Visibility.Visible;
            Btn1.Content = btn1Text;
            Btn2.Content = btn2Text;
            Btn1.Click += btn1Action;
            Btn2.Click += btn2Action;
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            FadeIn();

            if(_autoHide) {
                await Task.Delay(3000);

                FadeOut();
            }
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
                base.Close();
            };

            BeginAnimation(LeftProperty, slideOutX);
            BeginAnimation(OpacityProperty, fadeOut);
        }
    }
}
