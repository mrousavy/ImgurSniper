using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using ImgurSniper.Properties;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ImgurSniper.Libraries.Helper {
    public class ImgurUploader {
        private readonly ImgurClient _client;

        public static string ClientId => "766263aaa4c9882";

        public static string ClientSecret => "1f16f21e51e499422fb90ae670a1974e88f7c6ae";

        /// <summary>
        /// Login to Imgur with OAuth2
        /// </summary>
        public ImgurUploader() {
            _client = new ImgurClient(ClientId, ClientSecret);
        }

        //Login to Imgur Account
        public async Task Login() {
            try {
                string refreshToken = ConfigHelper.ReadRefreshToken();
                if (string.IsNullOrWhiteSpace(refreshToken))
                    return;

                OAuth2Endpoint endpoint = new OAuth2Endpoint(_client);
                IOAuth2Token token = await endpoint.GetTokenByRefreshTokenAsync(refreshToken);
                _client.SetOAuth2Token(token);
            } catch {
                // ignored
            }
        }

        /// <summary>
        /// Upload Image to Imgur
        /// </summary>
        /// <param name="bimage">The Image as byte[]</param>
        /// <param name="windowName">The name of the Window</param>
        /// <returns>The Link to the uploaded Image</returns>
        public async Task<string> Upload(byte[] bimage, string windowName = null) {
            int threadId = Thread.CurrentThread.ManagedThreadId;

            ImgurClient client = new ImgurClient(ClientId, ClientSecret);

            ImageEndpoint endpoint = new ImageEndpoint(client);

            string title = string.IsNullOrWhiteSpace(windowName)
                ? strings.uploadTitle
                : $"{windowName}  -  ({strings.uploadTitle})";

            IImage image = await endpoint.UploadImageBinaryAsync(bimage, null, title, "https://mrousavy.github.io/ImgurSniper");
            return image.Link;
        }

        /// <summary>
        /// Upload Image to Imgur Album
        /// </summary>
        /// <param name="bimage">The Image as byte[]</param>
        /// <param name="windowName">The name of the Window</param>
        /// <param name="albumId">The ID of the Album, or for anonymous Albums: DeleteHash</param>
        public async Task UploadToAlbum(byte[] bimage, string windowName, string albumId) {
            ImageEndpoint endpoint = new ImageEndpoint(_client);

            string title = string.IsNullOrWhiteSpace(windowName)
                ? strings.uploadTitle
                : $"{windowName}  -  ({strings.uploadTitle})";
            await endpoint.UploadImageBinaryAsync(bimage, albumId);
        }

        /// <summary>
        /// Create a New Album and get Link and AlbumID/DeleteHash
        /// </summary>
        /// <returns>Album Link and Album ID (or Album DeleteHash, if not logged in)</returns>
        public async Task<Tuple<string, string>> CreateAlbum() {
            AlbumEndpoint endpoint = new AlbumEndpoint(_client);
            IAlbum album = await endpoint.CreateAlbumAsync("Images uploaded with ImgurSniper",
                "https://mrousavy.github.io/ImgurSniper");

            Tuple<string, string> pair;

            //Logged in User = Item2 = Album ID
            if (_client.OAuth2Token != null) {
                pair = new Tuple<string, string>(album.Id, album.Id);
            }
            //Not Logged in User = Item2 = Album Delete Hash (Anonymous Albums)
            else {
                pair = new Tuple<string, string>(album.Id, album.DeleteHash);
            }
            return pair;
        }
    }
}