using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using ImgurSniper.Properties;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImgurSniper.Libraries.Helper {
    public class ImgurUploader {
        private readonly ImgurClient _client;

        public static string ClientId => "766263aaa4c9882";

        public static string ClientSecret => "1f16f21e51e499422fb90ae670a1974e88f7c6ae";

        /// <summary>
        ///     Login to Imgur with OAuth2
        /// </summary>
        public ImgurUploader() {
            _client = new ImgurClient(ClientId, ClientSecret);
        }

        //Login to Imgur Account
        public async Task Login() {
            try {
                OAuth2Endpoint endpoint = new OAuth2Endpoint(_client);

                string refreshToken = ConfigHelper.ReadRefreshToken();

                if (string.IsNullOrWhiteSpace(refreshToken)) {
                    IOAuth2Token token = await endpoint.GetTokenByRefreshTokenAsync(refreshToken);
                    _client.SetOAuth2Token(token);
                }
            } catch {
                // ignored
            }
        }

        /// <summary>
        ///     Upload Image to Imgur
        /// </summary>
        /// <param name="bimage">The Image as byte[]</param>
        /// <param name="windowName">The name of the Window</param>
        /// <returns>The Link to the uploaded Image</returns>
        public async Task<string> Upload(byte[] bimage, string windowName) {
            ImageEndpoint endpoint = new ImageEndpoint(_client);

            IImage image;
            using (MemoryStream stream = new MemoryStream(bimage)) {
                string title = string.IsNullOrWhiteSpace(windowName)
                    ? strings.uploadTitle
                    : $"{windowName}  -  ({strings.uploadTitle})";
                image = await endpoint.UploadImageStreamAsync(stream, null,
                    title,
                    "https://mrousavy.github.io/ImgurSniper");
            }
            return image.Link;
        }

        /// <summary>
        ///     Create a New Album and get Id
        /// </summary>
        /// <returns>Album ID and DeleteHash (If not logged in)</returns>
        public async Task<KeyValuePair<string, string>> CreateAlbum() {
            AlbumEndpoint endpoint = new AlbumEndpoint(_client);
            IAlbum album = await endpoint.CreateAlbumAsync("Images uploaded with ImgurSniper",
                "https://mrousavy.github.io/ImgurSniper");

            KeyValuePair<string, string> pair;

            //Logged in User = Album ID for uploads
            if (_client.OAuth2Token != null) {
                pair = new KeyValuePair<string, string>(album.Id, album.Id);
            }
            //Not Logged in User = Album Delete Has (Anonymous Albums)
            else {
                pair = new KeyValuePair<string, string>(album.Id, album.DeleteHash);
            }

            return pair;
        }
    }
}