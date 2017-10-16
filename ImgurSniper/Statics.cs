using ImgurSniper.Libraries.Helper;
using System;
using System.Threading.Tasks;

namespace ImgurSniper {
    public static class Statics {
        private static ImgurUploader _client { get; set; }

        public async static Task<ImgurUploader> GetUploaderAsync() {
            if (_client == null)
                _client = new ImgurUploader();

            await _client.Login(); // Will not do anything if token is still valid
            return _client;
        }

        public static NotificationWindow Notification {
            get => _notification;
            set {
                if (value == null) {
                    _notification = null;
                } else {
                    _notification?.Close();
                    _notification = value;
                }
            }
        }

        private static NotificationWindow _notification;

        public static void ShowNotification(string text, NotificationWindow.NotificationType type, bool autoHide, Action onClick = null) {
            Notification = new NotificationWindow(text, type, autoHide, onClick);
            Notification.Show();
        }

        public static async Task ShowNotificationAsync(string text, NotificationWindow.NotificationType type, Action onClick = null) {
            Notification = new NotificationWindow(text, type, true, onClick);
            await Notification.ShowAsync();
        }
    }
}
