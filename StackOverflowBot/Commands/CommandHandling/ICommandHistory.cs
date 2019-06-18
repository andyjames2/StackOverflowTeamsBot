using Microsoft.Bot.Builder;

namespace StackOverflowBot.Commands.CommandHandling
{
    public interface ICommandHistory
    {
        ICommand Pop(ITurnContext turnContext);
        void Push(ITurnContext turnContext, ICommand command);
    }
}