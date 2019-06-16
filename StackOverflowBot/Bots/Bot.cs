using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Command = StackOverflowBot.Commands.Command;

namespace StackOverflowBot.Bots
{
    public class Bot : ActivityHandler
    {

        private readonly IServiceProvider _serviceProvider;

        public Bot(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type != ActivityTypes.Message) {
                return;
            }
            var input = turnContext.Activity.Text;
            input = new Regex(@"<at>.*<\/at>").Replace(input, "").Trim(' ', (char)160, '\n');
            var commandBreakUp = input.Split(" ");
            var command = commandBreakUp.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(command))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
            }
            var args = commandBreakUp.Skip(1);
            await Command.Execute(this._serviceProvider, turnContext, cancellationToken, command, args);
        }

    }
}
