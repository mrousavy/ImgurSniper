using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ImgurSniper.UI {
    internal static class Animations {
        public static DoubleAnimation GetBrightenAnimation(double opacity) {
            return new DoubleAnimation {
                To = 1,
                From = opacity,
                Duration = TimeSpan.FromMilliseconds(300)
            };
        }
        public static DoubleAnimation GetDarkenAnimation(double opacity) {
            return new DoubleAnimation {
                To = 0.5,
                From = opacity,
                Duration = TimeSpan.FromMilliseconds(100)
            };
        }


        //Animation Templates
        internal static DoubleAnimation FadeOut {
            get {
                DoubleAnimation anim = new DoubleAnimation {
                    From = 1,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.18))
                };
                return anim;
            }
        }
        internal static DoubleAnimation FadeIn {
            get {
                DoubleAnimation anim = new DoubleAnimation {
                    From = 0,
                    To = 1,
                    Duration = new Duration(TimeSpan.FromSeconds(0.18))
                };
                return anim;
            }
        }
    }
}