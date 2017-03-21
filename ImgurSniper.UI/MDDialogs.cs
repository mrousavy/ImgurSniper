using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using str = ImgurSniper.UI.Properties.strings;

namespace ImgurSniper.UI {
    public partial class MainWindow {
        #region Dialogs
        //Show a Material Design Yes/No Dialog
        private async Task<bool> ShowAskDialog(string message) {
            bool choice = false;

            CloseDia();

            StackPanel vPanel = new StackPanel {
                Margin = new Thickness(5)
            };

            StackPanel hPanel = new StackPanel {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            Label label = new Label {
                Content = message,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Button yes = new Button {
                Content = str.yes,
                Foreground = Brushes.Gray,
                Width = 60,
                Margin = new Thickness(3),
                Style = (Style)FindResource("MaterialDesignFlatButton")
            };
            yes.Click += delegate {
                choice = true;
                CloseDia();
            };
            Button no = new Button {
                Content = str.no,
                Foreground = Brushes.Gray,
                Width = 60,
                Margin = new Thickness(3),
                Style = (Style)FindResource("MaterialDesignFlatButton")
            };
            no.Click += delegate {
                choice = false;
                CloseDia();
            };

            hPanel.Children.Add(yes);
            hPanel.Children.Add(no);

            vPanel.Children.Add(label);
            vPanel.Children.Add(hPanel);

            await DialogHost.ShowDialog(vPanel);

            return choice;
        }


        //Show a Material Design Ok Dialog
        private async Task ShowOkDialog(string title, string message) {
            CloseDia();

            StackPanel vPanel = new StackPanel {
                Margin = new Thickness(5)
            };

            Label header = new Label {
                Content = title,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5),
                FontWeight = FontWeights.Bold,
                FontSize = FontSize + 1
            };

            Label content = new Label {
                Content = message,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5)
            };

            Button ok = new Button {
                Content = str.ok,
                Foreground = Brushes.Gray,
                Width = 60,
                Margin = new Thickness(3),
                Style = (Style)FindResource("MaterialDesignFlatButton"),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            ok.Click += delegate {
                CloseDia();
            };

            vPanel.Children.Add(header);
            vPanel.Children.Add(content);
            vPanel.Children.Add(ok);

            await DialogHost.ShowDialog(vPanel);
        }

        //Show a Material Design Progressbar Dialog
        private StackPanel ShowProgressDialog() {
            CloseDia();

            StackPanel vpanel = new StackPanel {
                Margin = new Thickness(10)
            };

            Label label = new Label {
                Content = str.downloadingUpdate,
                FontSize = 13,
                Foreground = Brushes.Gray
            };

            ProgressBar bar = new ProgressBar {
                Margin = new Thickness(3),
                IsIndeterminate = false,
                Minimum = 0,
                Maximum = 100
            };

            vpanel.Children.Add(label);
            vpanel.Children.Add(bar);

            DialogHost.ShowDialog(vpanel);

            return vpanel;
        }

        //Close the Material Design Dialog
        private void CloseDia() {
            DialogHost.CloseDialogCommand.Execute(null, DialogHost);
        }
        #endregion
    }
}
