using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using StackOverflowBot.Commands.CommandHandling;
using ICommand = StackOverflowBot.Commands.ICommand;

namespace StackOverflowBot.Bots
{
    public class Bot : ActivityHandler
    {

        private readonly ICommandBroker _commandBroker;

        public Bot(ICommandBroker commandBroker)
        {
            this._commandBroker = commandBroker;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type != ActivityTypes.Message) {
                return;
            }
            var input = turnContext.Activity.Text;
            input = new Regex(@"<at>.*<\/at>").Replace(input, "").Trim(' ', (char)160, '\n', '\r');
            var commandBreakUp = input.Split(" ");
            var command = commandBreakUp.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(command))
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
            }
            var args = commandBreakUp.Skip(1);
            await this._commandBroker.Execute(turnContext, cancellationToken, command, args);
        }

    }
}
