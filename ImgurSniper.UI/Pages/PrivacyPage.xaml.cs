using System.Diagnostics;
using System.Windows.Controls;

namespace ImgurSniper.UI.Pages {
    /// <summary>
    ///     Interaction logic for PrivacyPage.xaml
    /// </summary>
    public partial class PrivacyPage : Page {
        public PrivacyPage() {
            InitializeComponent();
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Process.Start("https://github.com/mrousavy/ImgurSniper");
        }
    }
}