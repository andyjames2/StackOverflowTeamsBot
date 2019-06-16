using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace StackOverflowBot.Commands
{
    public class NoCommand : Command
    {

        private readonly ITurnContext _turnContext;
        private readonly CancellationToken _cancellationToken;

        public NoCommand(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            this._turnContext = turnContext;
            this._cancellationToken = cancellationToken;
        }

        public override async Task<bool> Do(IEnumerable<string> args)
        {
            await this._turnContext.SendActivityAsync($"Sorry, I don't know what you mean by that, type 'help' to see what I can do.", cancellationToken: this._cancellationToken);
            return false;
        }

        public override async Task Undo()
        {
        }

    }
}
