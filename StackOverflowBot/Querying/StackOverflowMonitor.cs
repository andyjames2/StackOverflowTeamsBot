﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackOverflowBot.Links;
using StackOverflowBot.Querying.Model;
using StackOverflowBot.Repositories;
using StackOverflowBot.Subscriptions;

namespace StackOverflowBot.Querying
{
    public class StackOverflowMonitor : IStackOverflowMonitor, IDisposable
    {

        private Timer _timer;
        private readonly string _key;
        private readonly IConfiguration _configuration;
        private readonly IRepository<Link> _linkRepository;
        private readonly IRepository<Subscription> _subscriptionRepository;
        private readonly HttpClient _httpClient;

        public StackOverflowMonitor(IConfiguration configuration, IRepository<Link> linkRepository, IRepository<Subscription> subscriptionRepository, HttpClient httpClient)
        {
            var interval = configuration.GetValue<long>("QueryIntervalInMs", 0);
            if (interval == 0) interval = 10000;
            this._timer = new Timer(interval) { AutoReset = false };
            this._timer.Elapsed += async (s, e) => await this.Query();
            this._key = configuration.GetValue<string>("StackOverflowKey");
            this._configuration = configuration;
            this._linkRepository = linkRepository;
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

            foreach (var link in this._linkRepository.Get().Where(r => r.State == LinkState.Ready))
            {
                var sinceDate = (int) link.LastCheck.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                var questions = await link.GetRecentQuestionsAsync(this._httpClient, this._key, sinceDate);
                
                if (questions.Any())
                {
                    var subscriptions = this._subscriptionRepository.Get().Where(s => s.ServiceUrl == link.Target.ServiceUrl);
                    foreach (var question in questions)
                    foreach (var subscription in subscriptions)
                        await subscription.ReceiveQuestion(appId, appPassword, question);
                }

                link.LastCheck = DateTime.UtcNow;
                this._linkRepository.SaveOrUpdate(link);

            }
            this._timer.Start();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    this._timer.Stop();
                    this._timer.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(true);
        }
        #endregion

    }
}