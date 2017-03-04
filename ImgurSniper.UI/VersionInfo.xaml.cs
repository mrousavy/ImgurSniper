using Octokit;
using System.Collections.Generic;
using System.Windows;

namespace ImgurSniper.UI {
    /// <summary>
    /// Interaction logic for VersionInfo.xaml
    /// </summary>
    public partial class VersionInfo : Window {
        IReadOnlyList<GitHubCommit> _commits;

        public VersionInfo(IReadOnlyList<GitHubCommit> commits, int currentCommits) {
            InitializeComponent();


            for(int i = _commits.Count; i > currentCommits; i--) {
                listview.Items.Add(new System.Windows.Controls.Label() {
                    Content = i + ": " + _commits[i].Commit.Message
                });
            }

            _commits = commits;



        }

        private void YesClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void NoClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
