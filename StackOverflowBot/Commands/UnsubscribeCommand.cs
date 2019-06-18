using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema.Teams;
using StackOverflowBot.Repositories;
using StackOverflowBot.Subscriptions;

namespace StackOverflowBot.Commands
{
    public class UnsubscribeCommand : ICommand
    {

        private IRepository<Subscription> _repository;
        private readonly ITurnContext _turnContext;
        private readonly CancellationToken _cancellationToken;

        private Subscription _subscription;

        public UnsubscribeCommand(IRepository<Subscription> repository, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            this._repository = repository;
            this._turnContext = turnContext;
            this._cancellationToken = cancellationToken;
        }

        public async Task<bool> Do(IEnumerable<string> tags)
        {
            var (target, targetType) = this.GetTarget();
            this._subscription = this._repository.Get().FirstOrDefault(sub => sub.Target == target);

            var subscriberTerm = targetType == TargetType.TeamsChannel ? "this channel" : "you";
            if (this._subscription == null)
            {
                await this._turnContext.SendActivityAsync($"There doesn't seem to be a subscription for {subscriberTerm}.", cancellationToken: this._cancellationToken);
                return false;
            }

            this._repository.Delete(this._subscription);

            await this._turnContext.SendActivityAsync($"I've unsubscribed {subscriberTerm} from all posts. :(", cancellationToken: this._cancellationToken);
            return true;
        }

        private (string target, TargetType targetType) GetTarget()
        {
            var channelData = this._turnContext.Activity.GetChannelData<TeamsChannelData>();
            var isTeamsChannel = channelData.Channel != null;
            if (isTeamsChannel)
                return (channelData.Channel.Id, TargetType.TeamsChannel);
            else
                return (this._turnContext.Activity.Conversation.Id, TargetType.Conversation);
        }

        public async Task Undo()
        {
            this._repository.SaveOrUpdate(this._subscription);
            var subscriberTerm = this._subscription.TargetType == TargetType.TeamsChannel ? "this channel's" : "your";
            await this._turnContext.SendActivityAsync($"I've restored {subscriberTerm} subscription to all posts.");
        }

    }
}
