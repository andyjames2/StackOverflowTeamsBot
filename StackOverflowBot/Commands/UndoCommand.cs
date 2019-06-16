using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema.Teams;
using StackOverflowBot.Subscriptions;

namespace StackOverflowBot.Commands
{
    public class UndoCommand : Command
    {

        private readonly Dictionary<string, Stack<Command>> _history;
        private readonly ITurnContext _turnContext;
        private readonly CancellationToken _cancellationToken;

        public UndoCommand(Dictionary<string, Stack<Command>> history, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            this._history = history;
            this._turnContext = turnContext;
            this._cancellationToken = cancellationToken;
        }

        public override async Task<bool> Do(IEnumerable<string> args)
        {
            var (target, _) = this.GetTarget();
            if (!this._history.ContainsKey(target) || !this._history[target].Any())
            {
                await this._turnContext.SendActivityAsync($"There's nothing to undo. :/", cancellationToken: this._cancellationToken);
                return;
            }
            await this._history[target].Pop().Undo();
            return false;
        }

        public override async Task Undo() { }

        private (string target, TargetType targetType) GetTarget()
        {
            var channelData = this._turnContext.Activity.GetChannelData<TeamsChannelData>();
            var isTeamsChannel = channelData.Channel != null;
            if (isTeamsChannel)
                return (channelData.Channel.Id, TargetType.TeamsChannel);
            else
                return (this._turnContext.Activity.Conversation.Id, TargetType.Conversation);
        }

    }
}
