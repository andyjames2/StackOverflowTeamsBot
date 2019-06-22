using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackOverflowBot.Querying.Model;

namespace StackOverflowBot.Links
{
    public class Link : IEquatable<Link>
    {

        public string RegistrationKey { get; set; }

        public string TeamId { get; set; }

        public string AccessToken { get; set; }

        public DateTime Expiry { get; set; }

        public RegistrationState State { get; set; }

        public LinkTarget Target { get; set; }

        public DateTime LastCheck { get; set; } = DateTime.UtcNow;

        public bool Equals(Link other)
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

            this.State = RegistrationState.Ready;
        }

        public Task<IEnumerable<Question>> GetRecentQuestionsAsync(HttpClient client, string key, int sinceDate)
        {
            var query = $"order=desc&sort=creation&fromdate={sinceDate}";
            return this.GetAsync<Question>(client, key, query);
        }

        public Task<IEnumerable<Question>> GetLatestQuestionsAsync(HttpClient client, string key, int number)
        {
            var query = $"order=desc&sort=creation&pagesize={number}";
            return this.GetAsync<Question>(client, key, query);
        }

        private async Task<IEnumerable<T>> GetAsync<T>(HttpClient client, string key, string queryString)
        {
            var teamId = Uri.EscapeDataString(this.TeamId);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://api.stackexchange.com/2.2/questions?site=stackoverflow&key={key}&team=stackoverflow.com/c/{teamId}&filter=withbody&{queryString}");
            requestMessage.Headers.Add("X-API-Access-Token", this.AccessToken);
            var response = await client.SendAsync(requestMessage);
            var questionsJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StackOverflowResponse<T>>(questionsJson).Items;
        }

    }
}
