using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using StackOverflowBot.Commands.CommandHandling;
using ICommand = StackOverflowBot.Commands.ICommand;

namespace StackOverflowBot.Bots
{
    public class Bot : ActivityHandler
    {

        private readonly ICommandBroker _commandBroker;
        private readonly ILogger<Bot> logger;

        public Bot(ICommandBroker commandBroker, ILogger<Bot> logger)
        {
            this._commandBroker = commandBroker;
            this.logger = logger;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type != ActivityTypes.Message) {
                return;
            }
            try
            {
                await turnContext.SendActivityAsync(Activity.CreateTypingActivity(), cancellationToken);
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
            catch (Exception e)
            {
                await turnContext.SendActivityAsync("Sorry, I fell over and couldn't complete your request. x(", cancellationToken: cancellationToken);
                logger.LogError($"An exception occured trying to complete the request `{turnContext.Activity.Text}`.\n{e}", e);
            }
        }

    }
}
