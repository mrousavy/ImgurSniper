using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ImgurSniper.UI.Properties;
using MaterialDesignThemes.Wpf;

namespace ImgurSniper.UI {
    public static class DialogHelper {

        //Show a Material Design Yes/No Dialog
        public static async Task<bool> ShowAskDialog(this DialogHost host, string message) {
            bool choice = false;

            CloseDia(host);

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
                Content = strings.yes,
                Foreground = Brushes.Gray,
                Width = 60,
                Margin = new Thickness(3),
                Style = (Style)host.FindResource("MaterialDesignFlatButton")
            };
            yes.Click += delegate {
                choice = true;
                CloseDia(host);
            };
            Button no = new Button {
                Content = strings.no,
                Foreground = Brushes.Gray,
                Width = 60,
                Margin = new Thickness(3),
                Style = (Style)host.FindResource("MaterialDesignFlatButton")
            };
            no.Click += delegate {
                choice = false;
                CloseDia(host);
            };

            hPanel.Children.Add(yes);
            hPanel.Children.Add(no);

            vPanel.Children.Add(label);
            vPanel.Children.Add(hPanel);

            await host.ShowDialog(vPanel);

            return choice;
        }

        //Show a Material Design Ok Dialog
        public static async Task ShowOkDialog(this DialogHost host, string title, string message) {
            CloseDia(host);

            StackPanel vPanel = new StackPanel {
                Margin = new Thickness(5)
            };

            Label header = new Label {
                Content = title,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5),
                FontWeight = FontWeights.Bold
            };
            header.FontSize++;

            Label content = new Label {
                Content = message,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(5)
            };

            Button ok = new Button {
                Content = strings.ok,
                Foreground = Brushes.Gray,
                Width = 60,
                Margin = new Thickness(3),
                Style = (Style)host.FindResource("MaterialDesignFlatButton"),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            ok.Click += delegate { CloseDia(host); };

            vPanel.Children.Add(header);
            vPanel.Children.Add(content);
            vPanel.Children.Add(ok);

            await host.ShowDialog(vPanel);
        }

        //Show a Material Design Progressbar Dialog
        public static StackPanel ShowProgressDialog(this DialogHost host) {
            CloseDia(host);

            StackPanel vpanel = new StackPanel {
                Margin = new Thickness(10)
            };

            Label label = new Label {
                Content = strings.downloadingUpdate,
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

#pragma warning disable 4014
            host.ShowDialog(vpanel);
#pragma warning restore 4014

            return vpanel;
        }


        //Close the Material Design Dialog
        public static void CloseDia(this DialogHost host) {
            DialogHost.CloseDialogCommand.Execute(null, host);
        }
    }
}
