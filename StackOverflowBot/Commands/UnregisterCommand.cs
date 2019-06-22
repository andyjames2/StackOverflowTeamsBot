using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using StackOverflowBot.Links;
using StackOverflowBot.Repositories;

namespace StackOverflowBot.Commands
{
    public class UnlinkCommand : ICommand
    {

        private readonly IRepository<Link> _repository;
        private readonly ITurnContext _turnContext;
        private readonly CancellationToken _cancellationToken;
        private Link _existingRegistration;

        public UnlinkCommand(IRepository<Link> repository, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            this._repository = repository;
            this._turnContext = turnContext;
            this._cancellationToken = cancellationToken;
        }

        public async Task<bool> Do(IEnumerable<string> args)
        {
            if (args.Count() != 1)
            {
                await this._turnContext.SendActivityAsync($"I'll need to know the web address to your Stack Overflow Team, for example `unlink https://stackoverflow.com/c/contoso/`.", cancellationToken: this._cancellationToken);
                return false;
            }
            var teamIdRegex = new Regex(@"(?<=https?://(www.\\)?stackoverflow.com/c/)[a-zA-Z0-9-_]+?(?=/.*|$)");
            var teamIdMatch = teamIdRegex.Match(args.First());
            if (!teamIdMatch.Success)
            {
                await this._turnContext.SendActivityAsync($"Hmmm, I can't seem to work with that URL. Take the URL from the address bar in your browser while on your team's Stack Overflow, for example `unlink https://stackoverflow.com/c/contoso/`.", cancellationToken: this._cancellationToken);
                return false;
            }
            var teamId = teamIdMatch.Value.ToLower();

            this._existingRegistration = this._repository.Get().FirstOrDefault(r => r.TeamId == teamId);
            if (this._existingRegistration == null)
            {
                await this._turnContext.SendActivityAsync($"Sorry, you don't seem to have a link to the team `{teamId}`. :/", cancellationToken: this._cancellationToken);
                return false;
            }

            this._repository.Delete(this._existingRegistration);
            await this._turnContext.SendActivityAsync($"I've unlinked the Stack Overflow team `{teamId}` for you. :(", cancellationToken: this._cancellationToken);

            return true;
        }

        public async Task Undo()
        {
            await this._turnContext.SendActivityAsync($"I've relinked the Stack Overflow team `{this._existingRegistration.TeamId}`! :)", cancellationToken: this._cancellationToken);
            this._repository.SaveOrUpdate(this._existingRegistration);
        }

    }
}
