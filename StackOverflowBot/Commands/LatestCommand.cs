using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using StackOverflowBot.Querying.Model;
using StackOverflowBot.Links;
using StackOverflowBot.Repositories;
using StackOverflowBot.Subscriptions;

namespace StackOverflowBot.Commands
{
    public class LatestCommand : ICommand
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private IRepository<Link> _repository;
        private readonly ITurnContext _turnContext;
        private readonly CancellationToken _cancellationToken;

        private Subscription _subscription;

        public LatestCommand(HttpClient client, string apiKey, IRepository<Link> repository, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            this._httpClient = client;
            this._apiKey = apiKey;
            this._repository = repository;
            this._turnContext = turnContext;
            this._cancellationToken = cancellationToken;
        }

        public async Task<bool> Do(IEnumerable<string> args)
        {
            var number = 1;
            var arg = args.FirstOrDefault();
            if (arg != null && ! int.TryParse(arg, out number))
            {
                await this._turnContext.SendActivityAsync($"Sorry, I don't know what you mean. Tell how many of the latest questions you want. For example `latest 3`.", cancellationToken: this._cancellationToken);
                return false;
            }

            var questions = new List<IEnumerable<Question>>();
            var links = this._repository.Get().Where(t => t.Target.ServiceUrl == this._turnContext.Activity.ServiceUrl);
            foreach (var link in links)
            {
                var result = await link.GetLatestQuestionsAsync(this._httpClient, this._apiKey, number);
                questions.Add(result);
            }

            foreach (var questionStack in questions)
            foreach (var question in questionStack)
            {
                await this._turnContext.SendActivityAsync(question.ToString(), cancellationToken: this._cancellationToken);
            }
            return false;
        }

        public async Task Undo()
        {
        }

    }
}
