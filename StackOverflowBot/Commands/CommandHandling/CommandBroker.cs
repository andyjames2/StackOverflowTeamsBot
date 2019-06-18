using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace StackOverflowBot.Commands.CommandHandling
{
    public class CommandBroker : ICommandBroker
    {

        private readonly ICommandFactory _commandFactory;
        private readonly ICommandHistory _commandHistory;

        public CommandBroker(ICommandFactory commandFactory, ICommandHistory commandHistory)
        {
            this._commandFactory = commandFactory;
            this._commandHistory = commandHistory;
        }

        public async Task Execute(ITurnContext turnContext, CancellationToken cancellationToken, string commandStr, IEnumerable<string> args)
        {
            var command = this._commandFactory.Create(commandStr, turnContext, cancellationToken);
            var pushToHistory = await command.Do(args);
            if (pushToHistory)
                this._commandHistory.Push(turnContext, command);
        }

    }
}
