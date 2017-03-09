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

#if DEBUG
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

        private void NoClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}
