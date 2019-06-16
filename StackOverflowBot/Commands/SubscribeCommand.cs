using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema.Teams;
using StackOverflowBot.Subscriptions;

namespace StackOverflowBot.Commands
{
    public class SubscribeCommand : Command
    {

        private IRepository<Subscription> _repository;
        private readonly ITurnContext _turnContext;
        private readonly CancellationToken _cancellationToken;

        private Subscription _subscription;
        private IEnumerable<string> _previousTags;

        public SubscribeCommand(IRepository<Subscription> repository, ITurnContext turnContext, CancellationToken cancellationToken)
        {
            this._repository = repository;
            this._turnContext = turnContext;
            this._cancellationToken = cancellationToken;
        }

        public override async Task<bool> Do(IEnumerable<string> tags)
        {
            var (target, targetType) = this.GetTarget();
            this._subscription = this._repository.Get().FirstOrDefault(sub => sub.Target == target);

            this._previousTags = this._subscription?.Tags;
            if (this._subscription != null)
                this._subscription.UpdateTags(tags);
            else
                this._subscription = this.CreateSubscription(target, targetType, tags);
            this._repository.SaveOrUpdate(this._subscription);

            var subscriberTerm = targetType == TargetType.TeamsChannel ? "this channel" : "you";
            if (tags.Any())
                await this._turnContext.SendActivityAsync($"Ok! I've subscribed {subscriberTerm} to posts that have any of the following tags: {string.Join(", ", this._subscription.Tags)}.", cancellationToken: this._cancellationToken);
            else
                await this._turnContext.SendActivityAsync($"Alright, I've subscribed {subscriberTerm} to all posts!", cancellationToken: this._cancellationToken);

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

        private Subscription CreateSubscription(string target, TargetType targetType, IEnumerable<string> tags)
        {
            var subscription = new Subscription();
            subscription.PlatformId = this._turnContext.Activity.ChannelId;
            subscription.ServiceUrl = this._turnContext.Activity.ServiceUrl;
            subscription.Bot = this._turnContext.Activity.Recipient;
            subscription.Subscriber = this._turnContext.Activity.From;
            subscription.Target = target;
            subscription.TargetType = targetType;
            subscription.Tags = tags;

            return subscription;
        }

        public override async Task Undo()
        {
            if (this._previousTags == null)
            {
                var subscriberTerm = this._subscription.TargetType == TargetType.TeamsChannel ? "this channel" : "you";
                this._repository.Delete(this._subscription);
                await this._turnContext.SendActivityAsync($"I've unsubscribed {subscriberTerm}.");
            }
            else
            {
                this._subscription.Tags = this._previousTags;
                this._repository.SaveOrUpdate(this._subscription);

                var subscriberTerm = this._subscription.TargetType == TargetType.TeamsChannel ? "this channel's" : "your";
                if (this._previousTags.Any())
                    await this._turnContext.SendActivityAsync($"I've restored {subscriberTerm} subscriptions to posts that have any of the following tags: {string.Join(", ", this._previousTags)}.", cancellationToken: this._cancellationToken);
                else
                    await this._turnContext.SendActivityAsync($"I've restored {subscriberTerm} subscription to all posts.");

            }
        }

    }
}
