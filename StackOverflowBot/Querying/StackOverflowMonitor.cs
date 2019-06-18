using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackOverflowBot.Registrations;
using StackOverflowBot.Repositories;

namespace StackOverflowBot.Querying
{
    public class StackOverflowMonitor : IStackOverflowMonitor
    {

        private Timer _timer;
        private readonly IConfiguration _configuration;
        private readonly string _key;
        private readonly IRepository<Registration> _repository;
        private readonly HttpClient _httpClient;

        public StackOverflowMonitor(IConfiguration configuration, IRepository<Registration> repository, HttpClient httpClient)
        {
            this._timer = new Timer(async s => await this.Query());
            this._configuration = configuration;
            this._key = this._configuration.GetValue<string>("StackOverflowKey");
            this._repository = repository;
            this._httpClient = httpClient;
        }

        public void Start()
        {
            var interval = this._configuration.GetValue<long>("QueryIntervalInMs");
            this._timer.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(interval));
        }

        public async Task Query()
        {
            foreach (var registration in this._repository.Get().Where(r => r.AccessToken != null))
            // todo: Add a registration state
            {
                var teamId = Uri.EscapeDataString(registration.TeamId);
                this._httpClient.DefaultRequestHeaders.Add("X-API-Access-Token", registration.AccessToken);

                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://api.stackexchange.com/2.2/questions?order=desc&sort=creation&site=stackoverflow&key={this._key}&team=stackoverflow.com/c/{teamId}");
                requestMessage.Headers.Add("X-API-Access-Token", registration.AccessToken);
                var response = await this._httpClient.SendAsync(requestMessage);
                var questionsJson = await response.Content.ReadAsStringAsync();
                var allQuestions = JsonConvert.DeserializeObject<dynamic>(questionsJson);

            }
            // todo: need to add token refreshing
        }

    }
}