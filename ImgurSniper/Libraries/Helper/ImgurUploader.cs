using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using ImgurSniper.Properties;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImgurSniper.Libraries.Helper {
    public class ImgurUploader {
        private readonly ImgurClient _client;
        private IOAuth2Token _token;

        public static string ClientId => "766263aaa4c9882";

        public static string ClientSecret => "1f16f21e51e499422fb90ae670a1974e88f7c6ae";

        /// <summary>
        /// Login to Imgur with OAuth2
        /// </summary>
        public ImgurUploader() {
            _client = new ImgurClient(ClientId, ClientSecret);
            _token = null;
        }

        //Login to Imgur Account
        public async Task Login() {
            try {
                if (_token != null && _token.ExpiresIn > 60) 
                    return; // More than 60 seconds left; we don't need to login

                string refreshToken = ConfigHelper.ReadRefreshToken();
                if (string.IsNullOrWhiteSpace(refreshToken)) // No refresh token found; exit
                    throw new Exception("No refresh-token found!");

                // Refresh access token
                OAuth2Endpoint endpoint = new OAuth2Endpoint(_client);
                _token = await endpoint.GetTokenByRefreshTokenAsync(refreshToken);
                _client.SetOAuth2Token(_token);
            } catch {
                // ignored
            }
        }

        /// <summary>
        /// Upload Image to Imgur
        /// </summary>
        /// <param name="stream">The Image as any sort of Stream</param>
        /// <param name="windowName">The name of the Window</param>
        /// <returns>The Link to the uploaded Image</returns>
        public async Task<string> Upload(Stream stream, string windowName = null) {
            //Set Stream Position to beginning
            stream.Position = 0;

            ImageEndpoint endpoint = new ImageEndpoint(_client);

            string title = string.IsNullOrWhiteSpace(windowName)
                ? strings.uploadTitle
                : $"{windowName}  -  ({strings.uploadTitle})";

            //TODO: use title or null?
            IImage image = await endpoint.UploadImageStreamAsync(stream, null, null, "https://mrousavy.github.io/ImgurSniper");
            return image.Link;
        }

        /// <summary>
        /// Upload Image to Imgur Album
        /// </summary>
        /// <param name="stream">The Image as any sort of Stream</param>
        /// <param name="windowName">The name of the Window</param>
        /// <param name="albumId">The ID of the Album, or for anonymous Albums: DeleteHash</param>
        public async Task UploadToAlbum(Stream stream, string windowName, string albumId) {
            //Set Stream Position to beginning
            stream.Position = 0;

            ImageEndpoint endpoint = new ImageEndpoint(_client);

            string title = string.IsNullOrWhiteSpace(windowName)
                ? null
                : windowName;
            await endpoint.UploadImageStreamAsync(stream, albumId, title);
        }

        /// <summary>
        /// Create a New Album and get Link and AlbumID/DeleteHash
        /// </summary>
        /// <returns>Album Link and Album ID (or Album DeleteHash, if not logged in)</returns>
        public async Task<Tuple<string, string>> CreateAlbum() {
            AlbumEndpoint endpoint = new AlbumEndpoint(_client);
            IAlbum album = await endpoint.CreateAlbumAsync("Images uploaded with ImgurSniper",
                "https://mrousavy.github.io/ImgurSniper");

            //Logged in User = Item2 = Album ID
            Tuple<string, string> pair = _client.OAuth2Token != null ?
                new Tuple<string, string>(album.Id, album.Id) :
                new Tuple<string, string>(album.Id, album.DeleteHash);
            return pair;
        }
    }
}
