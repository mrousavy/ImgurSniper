using Octokit;
using System.Collections.Generic;
using System.Windows;
// ReSharper disable UnusedParameter.Local

namespace ImgurSniper.UI {
    /// <summary>
    ///     Interaction logic for VersionInfo.xaml
    /// </summary>
    public partial class VersionInfo {
        private readonly int _latest;
        public bool Skipped;

        public VersionInfo(IReadOnlyList<GitHubCommit> commits, int currentCommits) {
            InitializeComponent();
            _latest = commits.Count;

            //commits = correct values
            int commitNr = commits.Count;
            for (int i = 0; i < commits.Count - currentCommits; i++) {
                Commit commit = commits[i].Commit;
                Listview.Items.Add(new VersionInfoItem {
                    Version = "v" + commitNr,
                    Date = $"{commit.Author.Date:dd.MM}",
                    Message = $"\"{commit.Message}\""
                });

                commitNr--;
            }
        }

        private void YesClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void SkipClick(object sender, RoutedEventArgs e) {
            Skipped = true;
            ConfigHelper.CurrentCommits = _latest;
            ConfigHelper.UpdateAvailable = false;
            ConfigHelper.Save();
            DialogResult = false;
        }

        private void NoClick(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}