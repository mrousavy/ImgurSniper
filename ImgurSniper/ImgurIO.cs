using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using System.IO;
using System.Threading.Tasks;

namespace ImgurSniper {
    public class ImgurIO {
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

        /// <summary>
        /// Login to Imgur with OAuth2
        /// </summary>
        public ImgurIO() {
            _client = new ImgurClient(ClientID, ClientSecret);

            if(FileIO.TokenExists)
                Login();
        }


        private async void Login() {
            try {
                OAuth2Endpoint endpoint = new OAuth2Endpoint(_client);

                string refreshToken = FileIO.ReadRefreshToken();
                IOAuth2Token token = await endpoint.GetTokenByRefreshTokenAsync(refreshToken);

                _client.SetOAuth2Token(token);
            } catch {
                // ignored
            }
        }

        /// <summary>
        /// Upload Image to Imgur
        /// </summary>
        /// <param name="image">The Image as byte[]</param>
        /// <returns>The Link to the uploaded Image</returns>
        public async Task<string> Upload(byte[] bimage, string WindowName) {
            ImageEndpoint endpoint = new ImageEndpoint(_client);

            IImage image;
            using(MemoryStream stream = new MemoryStream(bimage)) {
                string title = string.IsNullOrWhiteSpace(WindowName) ?
                    Properties.strings.uploadTitle :
                    $"{WindowName}  -  (" + Properties.strings.uploadTitle + ")";
                image = await endpoint.UploadImageStreamAsync(stream, null,
                    title,
                    "https://mrousavy.github.io/ImgurSniper");
            }
            return image.Link;
        }
    }
}
