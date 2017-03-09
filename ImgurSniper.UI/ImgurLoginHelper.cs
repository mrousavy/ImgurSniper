using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using ImgurSniper.UI.Properties;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Toast;

namespace ImgurSniper.UI {
    internal class ImgurLoginHelper {

        public static string ClientId => "766263aaa4c9882";

        public static string ClientSecret => "1f16f21e51e499422fb90ae670a1974e88f7c6ae";

        public string User { private set; get; }

        private readonly ImgurClient _client;
        private readonly OAuth2Endpoint _endpoint;

        private readonly Toasty _error;
        private readonly Toasty _success;

        /// <summary>
        /// Login to Imgur with OAuth2
        /// </summary>
        public ImgurLoginHelper(Toasty errorToast, Toasty successToast) {
            _client = new ImgurClient(ClientId, ClientSecret);
            _endpoint = new OAuth2Endpoint(_client);

            _error = errorToast;
            _success = successToast;
        }

        public void Authorize() {
            string redirectUrl = _endpoint.GetAuthorizationUrl(Imgur.API.Enums.OAuth2ResponseType.Pin);
            Process.Start(redirectUrl);

            _success.Show(strings.plsPin, TimeSpan.FromSeconds(2));
        }

        public async Task<bool> Login(string pin) {
            try {
                IOAuth2Token token = await _endpoint.GetTokenByPinAsync(pin);
                _client.SetOAuth2Token(token);

                FileIO.WriteRefreshToken(token.RefreshToken);

                User = token.AccountUsername;
                _success.Show(string.Format(strings.loggedIn, User), TimeSpan.FromSeconds(2));
                return true;
            } catch(Exception ex) {
                _error.Show(string.Format(strings.wrongPin, ex.Message), TimeSpan.FromSeconds(2));
                return false;
            }
        }

        public async Task<string> LoggedInUser(string refreshToken) {
            string username = null;

            try {
                IOAuth2Token token = await _endpoint.GetTokenByRefreshTokenAsync(refreshToken);
                username = token.AccountUsername;
            } catch { }

            return username;
        }
    }
}
