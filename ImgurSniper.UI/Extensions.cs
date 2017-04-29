using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace ImgurSniper.UI {
    public static class Extensions {

        #region UI
        public static async Task AnimateAsync(this UIElement element, DependencyProperty dp, double from, double to, int duration, int beginTime = 0) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            try {
                await element.Dispatcher.BeginInvoke(new Action(() => {
                    DoubleAnimation animation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(duration)) {
                        BeginTime = TimeSpan.FromMilliseconds(beginTime)
                    };
                    animation.Completed += delegate {
                        tcs.SetResult(true);
                    };
                    element.BeginAnimation(dp, animation);
                }));
            } catch {
                //Task was canceled
                return;
            }

            await tcs.Task;
        }

        public static async Task AnimateAsync(this UIElement element, DependencyProperty dp, DoubleAnimation animation) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            try {
                await element.Dispatcher.BeginInvoke(new Action(() => {
                    animation.Completed += delegate {
                        tcs.SetResult(true);
                    };
                    element.BeginAnimation(dp, animation);
                }));
            } catch {
                //Task was canceled
                return;
            }

            await tcs.Task;
        }

        public static async void Animate(this UIElement element, DependencyProperty dp, double from, double to, int duration, int beginTime = 0) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            try {
                await element.Dispatcher.BeginInvoke(new Action(() => {
                    DoubleAnimation animation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(duration)) {
                        BeginTime = TimeSpan.FromMilliseconds(beginTime)
                    };
                    animation.Completed += delegate {
                        tcs.SetResult(true);
                    };
                    element.BeginAnimation(dp, animation);
                }));
            } catch {
                //Task was canceled
                return;
            }

            await tcs.Task;
        }

        public static async void Animate(this UIElement element, DependencyProperty dp, DoubleAnimation animation) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            try {
                await element.Dispatcher.BeginInvoke(new Action(() => {
                    animation.Completed += delegate {
                        tcs.SetResult(true);
                    };
                    element.BeginAnimation(dp, animation);
                }));
            } catch {
                //Task was canceled
                return;
            }

            await tcs.Task;
        }
        #endregion
    }
}
