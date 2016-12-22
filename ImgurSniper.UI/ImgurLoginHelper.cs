using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Toast;

namespace ImgurSniper.UI {
    class ImgurLoginHelper {

        public static string ClientID {
            get {
                return "766263aaa4c9882";
            }
        }
        public static string ClientSecret {
            get {
                return "1f16f21e51e499422fb90ae670a1974e88f7c6ae";
            }
        }

        private ImgurClient _client;
        private OAuth2Endpoint _endpoint;

        private Toasty error, success;

        /// <summary>
        /// Login to Imgur with OAuth2
        /// </summary>
        public ImgurLoginHelper(Toasty errorToast, Toasty successToast) {
            _client = new ImgurClient(ClientID, ClientSecret);
            _endpoint = new OAuth2Endpoint(_client);

            error = errorToast;
            success = successToast;
        }

        public void Authorize() {
            string redirectUrl = _endpoint.GetAuthorizationUrl(Imgur.API.Enums.OAuth2ResponseType.Pin);
            Process.Start(redirectUrl);

            success.Show("Please enter the PIN you received on the Website!", TimeSpan.FromSeconds(2));
        }

        public async Task<bool> Login(string pin) {
            try {
                IOAuth2Token token = await _endpoint.GetTokenByPinAsync(pin);
                _client.SetOAuth2Token(token);

                FileIO.WriteRefreshToken(token.RefreshToken);

                success.Show("Successfully logged in! Hi, " + token.AccountUsername + "!", TimeSpan.FromSeconds(2));
                return true;
            } catch(Exception ex) {
                error.Show("Wrong PIN? Could not login to Imgur! (" + ex.Message + ")", TimeSpan.FromSeconds(2));
                return false;
            }
        }

        public async Task<string> LoggedInUser(string refreshToken) {
            string username = null;

            try {
                IOAuth2Token token = await _endpoint.GetTokenByRefreshTokenAsync(refreshToken);
                username = token.AccountUsername;
            } catch(Exception) { }

            return username;
        }
    }
}
