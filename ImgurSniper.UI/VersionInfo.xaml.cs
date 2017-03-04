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

            _commits = commits;

            for(int i = 0; i < _commits.Count - currentCommits; i++) {
                Commit commit = _commits[i].Commit;
                listview.Items.Add(new System.Windows.Controls.Label() {
                    Content = i + $" (@{commit.Author.Date}): " + commit.Message
                });
            }
        }

        private void YesClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void NoClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
