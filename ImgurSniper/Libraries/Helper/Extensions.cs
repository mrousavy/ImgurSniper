using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ImgurSniper.Libraries.Helper {
    public static class Extensions {
        public const int BufferSize = 4096;

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (T item in source) {
                action(item);
            }
        }

        public static string Replace(this string str, string oldValue, string newValue, StringComparison comparison) {
            if (string.IsNullOrEmpty(oldValue)) {
                return str;
            }

            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1) {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }

        public static async Task Animate(this Control control, DependencyProperty dp, double from, double to, int duration) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            DoubleAnimation animation = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(duration));
            animation.Completed += delegate {
                tcs.SetResult(true);
            };
            control.BeginAnimation(dp, animation);

            await tcs.Task;
        }

        public static async Task Animate(this Control control, DependencyProperty dp, DoubleAnimation animation) {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            animation.Completed += delegate {
                tcs.SetResult(true);
            };
            control.BeginAnimation(dp, animation);

            await tcs.Task;
        }
    }
}