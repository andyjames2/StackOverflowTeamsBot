using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StackOverflowBot.Registrations
{
    public class Registration : IEquatable<Registration>
    {

        public string RegistrationKey { get; set; }

        public string TeamId { get; set; }

        public string AccessToken { get; set; }

        public DateTime Expiry { get; set; }

        public RegistrationConfirmationTarget ConfirmationTarget { get; set; }

        public bool Equals(Registration other)
        {
            return this.RegistrationKey == other.RegistrationKey;
        }

        public async Task GetAccessToken(HttpClient client, string clientId, string clientSecret, string code, string redirectUrl)
        {
            var requestValues = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUrl),
            };
            var responseMessage = await client.PostAsync("https://stackoverflow.com/oauth/access_token/json", new FormUrlEncodedContent(requestValues));

            var response = await responseMessage.Content.ReadAsAsync<Dictionary<string, string>>();

            this.AccessToken = response["access_token"];
            var expiresIn = long.Parse(response["expires"]);
            this.Expiry = DateTime.UtcNow.AddSeconds(expiresIn);
        }

    }
}
