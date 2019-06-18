using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema.Teams;

namespace StackOverflowBot.Commands.CommandHandling
{
    class CommandHistory : ICommandHistory
    {

        private Dictionary<string, Stack<ICommand>> _history = new Dictionary<string, Stack<ICommand>>();

        public void Push(ITurnContext turnContext, ICommand command)
        {
            var historyStack = this.GetHistoryStack(turnContext);
            historyStack.Push(command);
        }

        public ICommand Pop(ITurnContext turnContext)
        {
            var historyStack = this.GetHistoryStack(turnContext);
            if (historyStack.Count == 0)
                return null;
            return historyStack.Pop();
        }

        private Stack<ICommand> GetHistoryStack(ITurnContext turnContext)
        {
            var channelData = turnContext.Activity.GetChannelData<TeamsChannelData>();
            var isTeamsChannel = channelData.Channel != null;
            string target;
            if (isTeamsChannel)
                target = channelData.Channel.Id;
            else
                target = turnContext.Activity.Conversation.Id;

            Stack<ICommand> historyStack;
            if (_history.ContainsKey(target))
                historyStack = _history[target];
            else
                _history[target] = historyStack = new Stack<ICommand>();

            return historyStack;
        }

    }
}
