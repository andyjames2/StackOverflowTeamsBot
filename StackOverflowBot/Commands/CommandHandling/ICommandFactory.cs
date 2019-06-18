using System.Threading;
using Microsoft.Bot.Builder;

namespace StackOverflowBot.Commands.CommandHandling
{
    public interface ICommandFactory
    {

        ICommand Create(string command, ITurnContext turnContext, CancellationToken cancellationToken);

    }
}
