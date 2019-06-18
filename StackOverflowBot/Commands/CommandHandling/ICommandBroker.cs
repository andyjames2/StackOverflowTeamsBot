using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;

namespace StackOverflowBot.Commands.CommandHandling
{
    public interface ICommandBroker
    {

        Task Execute(ITurnContext turnContext, CancellationToken cancellationToken, string commandStr, IEnumerable<string> args);

    }
}