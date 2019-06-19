using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackOverflowBot.Querying.Model;
using StackOverflowBot.Registrations;
using StackOverflowBot.Repositories;
using StackOverflowBot.Subscriptions;

namespace StackOverflowBot.Querying
{
    public class StackOverflowMonitor : IStackOverflowMonitor
    {

        private Timer _timer;
        private readonly string _key;
        private readonly IConfiguration _configuration;
        private readonly IRepository<Registration> _registrationRepository;
        private readonly IRepository<Subscription> _subscriptionRepository;
        private readonly HttpClient _httpClient;

        public StackOverflowMonitor(IConfiguration configuration, IRepository<Registration> registrationRepository, IRepository<Subscription> subscriptionRepository, HttpClient httpClient)
        {
            var interval = configuration.GetValue<long>("QueryIntervalInMs", 0);
            if (interval == 0) interval = 10000;
            this._timer = new Timer(interval) { AutoReset = true };
            this._timer.Elapsed += async (s, e) => await this.Query();
            this._key = configuration.GetValue<string>("StackOverflowKey");
            this._configuration = configuration;
            this._registrationRepository = registrationRepository;
            this._subscriptionRepository = subscriptionRepository;
            this._httpClient = httpClient;
        }

        public void Start()
        {
            this._timer.Start();
        }

        public async Task Query()
        {
            var appId = this._configuration.GetValue<string>("MicrosoftAppId");
            var appPassword = this._configuration.GetValue<string>("MicrosoftAppPassword");

            foreach (var registration in this._registrationRepository.Get().Where(r => r.State == RegistrationState.Ready))
            {
                var teamId = Uri.EscapeDataString(registration.TeamId);
                var sinceDate = (int)registration.LastCheck.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                var questions = await GetRecentQuestions(registration, teamId, sinceDate);
                
                if (questions.Any())
                {
                    var subscriptions = this._subscriptionRepository.Get().Where(s => s.ServiceUrl == registration.Target.ServiceUrl);
                    foreach (var question in questions)
                    {
                        foreach (var subscription in subscriptions)
                        {
                            if (!subscription.Tags.Any() || question.Tags.Any(t => subscription.Tags.Contains(t)))
                            {
                                await subscription.Send(appId, appPassword, question);
                            }
                        }
                    }
                }

                registration.LastCheck = DateTime.UtcNow;
                this._registrationRepository.SaveOrUpdate(registration);
            }
            // todo: need to add token refreshing
        }

        private async Task<IEnumerable<Question>> GetRecentQuestions(Registration registration, string teamId, int sinceDate)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://api.stackexchange.com/2.2/questions?order=desc&sort=creation&fromdate={sinceDate}&site=stackoverflow&key={this._key}&team=stackoverflow.com/c/{teamId}");
            requestMessage.Headers.Add("X-API-Access-Token", registration.AccessToken);
            var response = await this._httpClient.SendAsync(requestMessage);
            var questionsJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StackOverflowResponse<Question>>(questionsJson).Items;
        }
    }
}