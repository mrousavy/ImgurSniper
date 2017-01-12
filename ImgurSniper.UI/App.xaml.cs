using System.Windows;

namespace ImgurSniper.UI {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public App() : base() {
            this.DispatcherUnhandledException += delegate {
                MessageBox.Show("An unknown Error occured in ImgurSniper.UI!\nImgurSniper has to shut down!");
            };
        }
    }
}
