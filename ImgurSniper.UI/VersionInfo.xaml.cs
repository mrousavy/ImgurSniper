using Octokit;
using System.Collections.Generic;
using System.Windows;

namespace ImgurSniper.UI {
    /// <summary>
    /// Interaction logic for VersionInfo.xaml
    /// </summary>
    public partial class VersionInfo : Window {
        private IReadOnlyList<GitHubCommit> _commits;
        private int latest;

        public VersionInfo(IReadOnlyList<GitHubCommit> commits, int currentCommits) {
            InitializeComponent();

            _commits = commits;

#if DEBUG
            latest = 50;

            //commits = null and currentCommits = ?? in DEBUG
            int commitNr = 50;
            for(int i = 0; i < 50 - 4; i++) {
                listview.Items.Add(new VersionInfoItem {
                    Version = "v" + commitNr,
                    Date = $"{System.DateTime.Now:dd.MM.yyyy}",
                    Message = "\"Updated Something!\""
                });

                commitNr--;
            }
#else
            latest = _commits.Count;

            //commits = correct values
            int commitNr = _commits.Count;
            for(int i = 0; i < _commits.Count - currentCommits; i++) {
                Commit commit = _commits[i].Commit;
                listview.Items.Add(new VersionInfoItem {
                    Version = "v" + commitNr,
                    Date = $"{commit.Author.Date:dd.MM.yyyy}",
                    Message = $"\"{commit.Message}\""
                });

                commitNr--;
            }
#endif
        }

        private void YesClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void SkipClick(object sender, RoutedEventArgs e) {
            FileIO.CurrentCommits = latest;
            FileIO.UpdateAvailable = false;
            DialogResult = false;
        }

        private void NoClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
