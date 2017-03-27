using System.Windows;
using System.Windows.Input;

namespace ImgurSniper.UI {
    /// <summary>
    ///     Interaction logic for HotKeySelector.xaml
    /// </summary>
    public partial class HotKeySelector : Window {
        public Key key;

        public HotKeySelector() {
            InitializeComponent();
        }

        private void SelectKey(object sender, KeyEventArgs e) {
            key = e.Key;

            if (key == Key.Escape) {
                DialogResult = false;
            }
            else {
                DialogResult = true;
            }
        }
    }
}