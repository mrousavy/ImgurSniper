using System;
using System.Threading.Tasks;
using System.Timers;

namespace ImgurSniper {
    /// <summary>
    /// Interaction logic for GifOutline.xaml
    /// </summary>
    public partial class GifOutline {
        private int _current;

        public GifOutline(Templates.RECT size, int gifLength) {
            InitializeComponent();

            ProgressBar.Maximum = gifLength / 10;
            //Start at 1 Sec
            _current = gifLength / 100;

            UpdateProgress();

            Left = size.Left - 2;
            Top = size.Top - 2;
            Width = size.Width + 4;
            Height = size.Height + 4;

            Outline.Width = Width;
            Outline.Height = Height;

            //Space for ProgressBar
            Height += 30;
        }


        //TODO: Fix the incorrectness of this Progressbar (Is some Secs behind because of lag
        private async void UpdateProgress() {
            int max = (int)ProgressBar.Maximum;
            while(_current < max) {
                _current += 10;
                ProgressBar.Value = _current;
                await Task.Delay(100);
            }
        }
    }
}
