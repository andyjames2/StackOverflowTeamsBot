using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using MlkPwgen;
using StackOverflowBot.Links;
using StackOverflowBot.Repositories;

namespace StackOverflowBot.Commands
{
    public class LinkCommand : ICommand
    {
        private readonly string _rootUrl;
        private readonly IRepository<Link> _repository;
        private readonly ITurnContext _turnContext;
        private readonly CancellationToken _cancellationToken;
        private string _linkKey;

        public LinkCommand(string rootUrl, IRepository<Link> repository, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            this._rootUrl = rootUrl;
            this._repository = repository;
            if (!this._rootUrl.EndsWith("/")) this._rootUrl += "/";
            this._turnContext = turnContext;
            this._cancellationToken = cancellationToken;
        }

        public async Task<bool> Do(IEnumerable<string> args)
        {
            if (args.Count() != 1)
            {
                await this._turnContext.SendActivityAsync($"I'll need to know the web address to your Stack Overflow Team, for example `link https://stackoverflow.com/c/contoso/`.", cancellationToken: this._cancellationToken);
                return false;
            }
            var teamIdRegex = new Regex(@"(?<=https?://(www.\\)?stackoverflow.com/c/)[a-zA-Z0-9-_]+?(?=/.*|$)");
            var teamIdMatch = teamIdRegex.Match(args.First());
            if (!teamIdMatch.Success)
            {
                await this._turnContext.SendActivityAsync($"Hmmm, I can't seem to work with that URL. Take the URL from the address bar in your browser while on your team's Stack Overflow, for example `link https://stackoverflow.com/c/contoso/`.", cancellationToken: this._cancellationToken);
                return false;
            }
            var teamId = teamIdMatch.Value.ToLower();
            var linkKey = PasswordGenerator.Generate(length: 10, allowed: Sets.Alphanumerics);

            var existingLink = this._repository.Get().FirstOrDefault(r => r.Target.ServiceUrl == this._turnContext.Activity.ServiceUrl && r.TeamId == teamId);
            if (existingLink != null)
            {
                if (existingLink.State == LinkState.SettingUp)
                {
                    linkKey = existingLink.LinkKey;
                }
                else
                {
                    await this._turnContext.SendActivityAsync($"You already have a link to that team's Stack Overflow. If you want to change it inlink it first, for example `unlink https://stackoverflow.com/c/contoso/`.", cancellationToken: this._cancellationToken);
                    return false;
                }
            }


            this._repository.SaveOrUpdate(new Link() {
                LinkKey = linkKey,
                TeamId = teamId,
                Target = new LinkTarget
                {
                    Bot = this._turnContext.Activity.Recipient,
                    PlatformId = this._turnContext.Activity.ChannelId,
                    ServiceUrl = this._turnContext.Activity.ServiceUrl,
                    Id = this._turnContext.Activity.Conversation.Id
                }
            });

            this._linkKey = linkKey;

            await this._turnContext.SendActivityAsync($"Great, let's get started! Click the following link to authorize my access to your team's Stack Overflow: [{_rootUrl}so/link/{linkKey}]({_rootUrl}so/link/{linkKey}).", cancellationToken: this._cancellationToken);

            return true;
        }

        public async Task Undo()
        {
            var existingLink = this._repository.Get().FirstOrDefault(r => r.LinkKey == this._linkKey);
            this._repository.Delete(existingLink);
        }

    }
}
